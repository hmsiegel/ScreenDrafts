using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Enums;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Errors;

namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.GamePlay;

public sealed class GetGuestDraftGameplayTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  // ── Happy path: full response shape across a mix of pick states ─────────────

  [Fact]
  public async Task GetGameplay_HappyPath_ShouldReturnFullyPopulatedResponseWithMixedPickStatesAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, b, c, d) = await CreateGuestDraftWithMixedPickStatesAsync();

    // Reveal slot1 -- resolve the auto-assigned revealer from the response itself
    // (random draw among B/C/D with >2 participants). The participant-identity bug
    // is fixed: RevealAuthorizedParticipantId now matches a real participant's
    // ParticipantId in the same response, which cross-references back to a real
    // GuestDrafterPublicId -- no need to fall back to the domain aggregate.
    var beforeReveal = await GetGameplayAsync(guestDraftPublicId, owner.UserPublicId);
    beforeReveal.IsSuccess.Should().BeTrue();
    var slot1Pick = beforeReveal.Value.Picks.Single(p => p.PlayOrder == 1);
    slot1Pick.RevealAuthorizedParticipantId.Should().NotBeNull();

    var revealerFromResponse = beforeReveal.Value.Participants.Single(p =>
      p.ParticipantId == slot1Pick.RevealAuthorizedParticipantId
    );
    var candidates = new Dictionary<string, string>
    {
      [b.GuestDrafterPublicId] = b.UserPublicId,
      [c.GuestDrafterPublicId] = c.UserPublicId,
      [d.GuestDrafterPublicId] = d.UserPublicId,
    };
    revealerFromResponse.ParticipantPublicId.Should().BeOneOf(candidates.Keys);
    var revealerUserPublicId = candidates[revealerFromResponse.ParticipantPublicId];
    (await RevealPickAsync(guestDraftPublicId, 1, revealerUserPublicId))
      .IsSuccess.Should()
      .BeTrue();

    // Act
    var result = await GetGameplayAsync(guestDraftPublicId, owner.UserPublicId);

    // Assert -- top-level shape
    result.IsSuccess.Should().BeTrue();
    var response = result.Value;
    response.GuestDraftPublicId.Should().Be(guestDraftPublicId);
    response.Title.Should().NotBeNullOrWhiteSpace();
    response.Type.Should().Be(DraftType.MiniMega.Name);
    response.Status.Should().Be(DraftStatus.InProgress.Name);
    response
      .ShareToken.Should()
      .BeNull("no sharing feature exists yet, so the underlying column is always null");
    response.CallerContext.IsOwner.Should().BeTrue();
    response.CallerContext.IsParticipant.Should().BeTrue();
    response.CallerContext.ParticipantPublicId.Should().Be(owner.GuestDrafterPublicId);

    // Assert -- participants: every ParticipantPublicId/DisplayName resolves to a
    // real, known GuestDrafter (the identity bug's core fix).
    response.Participants.Should().HaveCount(4);
    var knownGuestDrafterPublicIds = new[]
    {
      owner.GuestDrafterPublicId,
      b.GuestDrafterPublicId,
      c.GuestDrafterPublicId,
      d.GuestDrafterPublicId,
    };
    foreach (var participant in response.Participants)
    {
      participant.ParticipantPublicId.Should().BeOneOf(knownGuestDrafterPublicIds);
      participant.DisplayName.Should().Be("Test User");
    }
    var ownerParticipantResponse = response.Participants.Single(p => p.IsOwner);
    ownerParticipantResponse.ParticipantPublicId.Should().Be(owner.GuestDrafterPublicId);

    // Assert -- positions: AssignedParticipantId resolves to a real participant
    // whose PublicId is one of the four known GuestDrafters.
    response.Positions.Should().HaveCount(4);
    foreach (var position in response.Positions)
    {
      position.AssignedParticipantId.Should().NotBeNull();
      var assignedParticipant = response.Participants.Single(p =>
        p.ParticipantId == position.AssignedParticipantId
      );
      assignedParticipant.ParticipantPublicId.Should().BeOneOf(knownGuestDrafterPublicIds);
      position.AssignedParticipantDisplayName.Should().Be("Test User");
    }

    // Assert -- picks, one per designed state
    response.Picks.Should().HaveCount(5);

    // Owner played every pick -- PlayedByParticipantId/PlayedByDisplayName must
    // resolve back to the owner's own participant, not just "some" participant.
    var slot1 = response.Picks.Single(p => p.Position == 1);
    slot1.PlayedByParticipantId.Should().Be(ownerParticipantResponse.ParticipantId);
    slot1.PlayedByDisplayName.Should().Be("Test User");
    slot1.IsRevealed.Should().BeTrue();
    slot1.MoviePublicId.Should().NotBeNull();
    slot1.WasVetoed.Should().BeFalse();
    slot1.WasVetoOverridden.Should().BeFalse();
    slot1.WasCommissionerOverride.Should().BeFalse();
    slot1.IsActiveOnFinalBoard.Should().BeTrue();
    slot1.RevealAuthorizedParticipantId.Should().Be(revealerFromResponse.ParticipantId);
    slot1.RevealAuthorizedByDisplayName.Should().Be("Test User");

    var slot2 = response.Picks.Single(p => p.Position == 2);
    slot2.IsRevealed.Should().BeFalse();
    slot2
      .MoviePublicId.Should()
      .NotBeNull("the owner can always see a pick's movie, revealed or not");
    slot2.IsActiveOnFinalBoard.Should().BeTrue();

    var slot3 = response.Picks.Single(p => p.Position == 3);
    slot3.WasCommissionerOverride.Should().BeTrue();
    slot3.IsActiveOnFinalBoard.Should().BeFalse();

    // slot4: vetoed by B, then overridden by C -- VetoedByDisplayName/
    // SavedByDisplayName must both resolve to a real name (never null/"Unknown"),
    // proving the JOIN to guest_drafters works for both roles.
    var slot4 = response.Picks.Single(p => p.Position == 4);
    slot4.WasVetoed.Should().BeFalse();
    slot4.WasVetoOverridden.Should().BeTrue();
    slot4.IsActiveOnFinalBoard.Should().BeTrue();
    slot4.VetoedByDisplayName.Should().Be("Test User");
    slot4.SavedByDisplayName.Should().Be("Test User");
    slot4.VetoHistory.Should().HaveCount(1);
    slot4.VetoHistory[0].Sequence.Should().Be(1);
    slot4.VetoHistory[0].IsOverridden.Should().BeTrue();
    slot4.VetoHistory[0].VetoedByDisplayName.Should().Be("Test User");
    slot4.VetoHistory[0].OverriddenByDisplayName.Should().Be("Test User");

    var slot5 = response.Picks.Single(p => p.Position == 5);
    slot5.WasVetoed.Should().BeTrue();
    slot5.IsActiveOnFinalBoard.Should().BeFalse();
    slot5
      .VetoHistory.Should()
      .HaveCount(2, "veto seq1 was overridden, then D self-overrode, then C vetoed again as seq2");
    slot5.VetoHistory.Select(v => v.Sequence).Should().ContainInOrder(1, 2);
    slot5.VetoHistory[0].IsOverridden.Should().BeTrue();
    slot5.VetoHistory[1].IsOverridden.Should().BeFalse();
  }

  // ── CallerContext ─────────────────────────────────────────────────────────

  [Fact]
  public async Task GetGameplay_AsOwner_ShouldReturnCorrectCallerContextAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _) = await CreateInProgressStandardGuestDraftAsync();
    var ownerGuestDrafterPublicId = await GetGuestDrafterPublicIdAsync(owner);

    // Act
    var result = await GetGameplayAsync(guestDraftPublicId, owner);

    // Assert -- the identity bug is fixed: CallerContext.ParticipantPublicId now
    // resolves via a direct DrafterUserId comparison, so the exact value can be
    // asserted against the real, known GuestDrafter public id.
    result.IsSuccess.Should().BeTrue();
    result.Value.CallerContext.IsOwner.Should().BeTrue();
    result.Value.CallerContext.IsParticipant.Should().BeTrue();
    result.Value.CallerContext.ParticipantPublicId.Should().Be(ownerGuestDrafterPublicId);
  }

  [Fact]
  public async Task GetGameplay_AsNonOwnerParticipant_ShouldReturnCorrectCallerContextAsync()
  {
    // Arrange
    var (guestDraftPublicId, _, other) = await CreateInProgressStandardGuestDraftAsync();
    var otherGuestDrafterPublicId = await GetGuestDrafterPublicIdAsync(other);

    // Act
    var result = await GetGameplayAsync(guestDraftPublicId, other);

    // Assert -- this specifically used to require a broken cross-referencing step
    // (the deleted GuestDraftParticipant.PublicId concept); it is now a direct
    // DrafterUserId comparison, confirmed here against a real value.
    result.IsSuccess.Should().BeTrue();
    result.Value.CallerContext.IsOwner.Should().BeFalse();
    result.Value.CallerContext.IsParticipant.Should().BeTrue();
    result.Value.CallerContext.ParticipantPublicId.Should().Be(otherGuestDrafterPublicId);
  }

  [Fact]
  public async Task GetGameplay_AsAuthenticatedUserWhoIsNotAParticipant_ShouldReturnNotFoundAsync()
  {
    // Arrange -- registered with the fake IUsersApi, but never invited to this draft
    var (guestDraftPublicId, _, _) = await CreateInProgressStandardGuestDraftAsync();
    var stranger = await CreateUserAsync();

    // Act
    var result = await GetGameplayAsync(guestDraftPublicId, stranger.UserPublicId);

    // Assert -- NotFound, not Forbidden, so a non-participant can't confirm a
    // private draft even exists
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DraftErrors.NotFound(guestDraftPublicId).Code);
  }

  [Fact]
  public async Task GetGameplay_WithNonExistentCaller_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, _, _) = await CreateInProgressStandardGuestDraftAsync();
    var nonExistentCaller = $"u_{Faker.Random.AlphaNumeric(15)}";

    // Act
    var result = await GetGameplayAsync(guestDraftPublicId, nonExistentCaller);

    // Assert
    result.IsFailure.Should().BeTrue();
    result
      .Errors.Should()
      .Contain(e => e.Code == UserPublicApiErrors.PublicIdNotFound(nonExistentCaller).Code);
  }

  [Fact]
  public async Task GetGameplay_WithNonExistentGuestDraft_ShouldReturnNotFoundAsync()
  {
    // Arrange
    var caller = await CreateUserAsync();
    var nonExistentGuestDraftPublicId = $"gd_{Faker.Random.AlphaNumeric(15)}";

    // Act
    var result = await GetGameplayAsync(nonExistentGuestDraftPublicId, caller.UserPublicId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result
      .Errors.Should()
      .Contain(e => e.Code == DraftErrors.NotFound(nonExistentGuestDraftPublicId).Code);
  }

  // ── ShareToken ────────────────────────────────────────────────────────────

  [Fact]
  public async Task GetGameplay_ShareTokenIsNull_ForBothOwnerAndNonOwnerParticipant_UntilSharingIsImplementedAsync()
  {
    // Arrange -- there is no share-token-generation feature yet, so the underlying
    // column is always null; this documents that the response's owner-only
    // exposure logic still runs correctly regardless (null either way today).
    var (guestDraftPublicId, owner, other) = await CreateInProgressStandardGuestDraftAsync();

    // Act
    var asOwner = await GetGameplayAsync(guestDraftPublicId, owner);
    var asOther = await GetGameplayAsync(guestDraftPublicId, other);

    // Assert
    asOwner.IsSuccess.Should().BeTrue();
    asOther.IsSuccess.Should().BeTrue();
    asOwner.Value.ShareToken.Should().BeNull();
    asOther.Value.ShareToken.Should().BeNull();
  }

  // ── Unrevealed-pick concealment ───────────────────────────────────────────

  [Fact]
  public async Task GetGameplay_UnrevealedPick_PickerCanSeeTheirOwnMovieAsync()
  {
    // Arrange
    var (guestDraftPublicId, _, picker, _, _, playOrder, moviePublicId) =
      await CreatePickWithDistinctRolesAsync();

    // Act
    var result = await GetGameplayAsync(guestDraftPublicId, picker);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var pick = result.Value.Picks.Single(p => p.PlayOrder == playOrder);
    pick.MoviePublicId.Should().Be(moviePublicId);
    pick.MovieTitle.Should().NotBeNull();
  }

  [Fact]
  public async Task GetGameplay_UnrevealedPick_DesignatedRevealerCanSeeTheMovieAsync()
  {
    // Arrange
    var (guestDraftPublicId, _, _, revealer, _, playOrder, moviePublicId) =
      await CreatePickWithDistinctRolesAsync();

    // Act
    var result = await GetGameplayAsync(guestDraftPublicId, revealer);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var pick = result.Value.Picks.Single(p => p.PlayOrder == playOrder);
    pick.MoviePublicId.Should().Be(moviePublicId);
    pick.MovieTitle.Should().NotBeNull();
  }

  [Fact]
  public async Task GetGameplay_UnrevealedPick_OwnerCanSeeTheMovieEvenIfNotPickerOrRevealerAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _, _, _, playOrder, moviePublicId) =
      await CreatePickWithDistinctRolesAsync();

    // Act
    var result = await GetGameplayAsync(guestDraftPublicId, owner);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var pick = result.Value.Picks.Single(p => p.PlayOrder == playOrder);
    pick.MoviePublicId.Should().Be(moviePublicId);
    pick.MovieTitle.Should().NotBeNull();
  }

  [Fact]
  public async Task GetGameplay_UnrevealedPick_EveryOtherParticipantCannotSeeTheMovieAsync()
  {
    // Arrange
    var (guestDraftPublicId, _, _, _, other, playOrder, _) =
      await CreatePickWithDistinctRolesAsync();

    // Act
    var result = await GetGameplayAsync(guestDraftPublicId, other);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var pick = result.Value.Picks.Single(p => p.PlayOrder == playOrder);
    pick.MoviePublicId.Should().BeNull();
    pick.MovieTitle.Should().BeNull();
  }

  [Fact]
  public async Task GetGameplay_OnceRevealed_EveryParticipantCanSeeTheMovieRegardlessOfRoleAsync()
  {
    // Arrange
    var (guestDraftPublicId, _, _, revealer, other, playOrder, moviePublicId) =
      await CreatePickWithDistinctRolesAsync();
    (await RevealPickAsync(guestDraftPublicId, playOrder, revealer)).IsSuccess.Should().BeTrue();

    // Act -- "other" could see nothing before the reveal
    var result = await GetGameplayAsync(guestDraftPublicId, other);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var pick = result.Value.Picks.Single(p => p.PlayOrder == playOrder);
    pick.IsRevealed.Should().BeTrue();
    pick.MoviePublicId.Should().Be(moviePublicId);
    pick.MovieTitle.Should().NotBeNull();
  }

  [Fact]
  public async Task GetGameplay_RevealedPick_ShouldIncludeMovieDetailsFromTheLocalCacheAsync()
  {
    // Arrange -- TmdbId/MovieYear/ImdbId come from the guest_drafts.movies JOIN,
    // not a cross-module lookup, so they must reflect whatever was cached there.
    // With exactly two participants, PlayPick auto-assigns "other" as the
    // designated revealer, not the picker themselves.
    var (guestDraftPublicId, owner, other) = await CreateInProgressStandardGuestDraftAsync();
    var moviePublicId = await CreateMovieAsync(tmdbId: 42, imdbId: "tt1234567", year: "1999");
    (await PlayPickAsync(guestDraftPublicId, owner, moviePublicId, 7, 1))
      .IsSuccess.Should()
      .BeTrue();
    (await RevealPickAsync(guestDraftPublicId, 1, other)).IsSuccess.Should().BeTrue();

    // Act
    var result = await GetGameplayAsync(guestDraftPublicId, owner);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var pick = result.Value.Picks.Single(p => p.PlayOrder == 1);
    pick.MoviePublicId.Should().Be(moviePublicId);
    pick.TmdbId.Should().Be(42);
    pick.ImdbId.Should().Be("tt1234567");
    pick.MovieYear.Should().Be("1999");
  }

  // ── Participant token balances ───────────────────────────────────────────

  [Fact]
  public async Task GetGameplay_ParticipantTokenBalances_ShouldMatchDomainArithmeticAsync()
  {
    // Arrange -- "other" spends their one starting veto; owner stays untouched
    var (guestDraftPublicId, owner, other) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, await CreateMovieAsync(), 7, 1);
    await ApplyVetoAsync(guestDraftPublicId, 1, other);

    // Act
    var result = await GetGameplayAsync(guestDraftPublicId, owner);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var ownerParticipant = result.Value.Participants.Single(p => p.IsOwner);
    ownerParticipant.VetoTokensRemaining.Should().Be(1);
    ownerParticipant.OverrideTokensRemaining.Should().Be(0);
    ownerParticipant.FungibleTokensRemaining.Should().Be(0);

    var otherParticipant = result.Value.Participants.Single(p => !p.IsOwner);
    otherParticipant
      .VetoTokensRemaining.Should()
      .Be(0, "StartingVetoes(1) + AwardedVetoes(0) - VetoesUsed(1)");
    otherParticipant.OverrideTokensRemaining.Should().Be(0);
    otherParticipant.FungibleTokensRemaining.Should().Be(0);
  }

  /// <summary>
  /// Four-participant (Owner, Picker, X, Y) MiniMega draft with one pick played by
  /// Picker, left unrevealed. The auto-assigned revealer is drawn randomly among
  /// {Owner, X, Y} (everyone except Picker); this retries the play (undo + replay
  /// with a fresh movie) until the draw lands on someone other than Owner, so the
  /// four roles this scenario exists to test -- Owner, Picker, Revealer, and an
  /// uninvolved "Other" participant -- are always four distinct people, regardless
  /// of which way the random draw actually goes.
  /// </summary>
  private async Task<(
    string GuestDraftPublicId,
    string Owner,
    string Picker,
    string Revealer,
    string Other,
    int PlayOrder,
    string MoviePublicId
  )> CreatePickWithDistinctRolesAsync()
  {
    var (guestDraftPublicId, users) = await CreateInProgressCustomGuestDraftAsync(
      4,
      DraftType.MiniMega
    );
    var owner = users[0];
    var picker = users[1];

    const int playOrder = 1;
    const int position = 1;
    string moviePublicId;
    TestUser revealer;

    var attempt = 0;
    while (true)
    {
      attempt++;
      moviePublicId = await CreateMovieAsync();
      (
        await PlayPickAsync(
          guestDraftPublicId,
          picker.UserPublicId,
          moviePublicId,
          position,
          playOrder
        )
      )
        .IsSuccess.Should()
        .BeTrue();

      var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
      var pick = guestDraft.Picks.Single(p => p.PlayOrder == playOrder);
      var revealerParticipant = guestDraft.Participants.Single(p =>
        p.Id == pick.RevealAuthorizedParticipantId
      );
      revealer = users.Single(u => u.GuestDrafterId == revealerParticipant.ParticipantIdValue);

      if (revealer != owner)
      {
        break;
      }

      attempt
        .Should()
        .BeLessThan(
          25,
          "the random draw excluding Owner should resolve within a handful of retries"
        );
      (await UndoPickAsync(guestDraftPublicId, playOrder, owner.UserPublicId))
        .IsSuccess.Should()
        .BeTrue();
    }

    var other = users.Single(u => u != owner && u != picker && u != revealer);

    return (
      guestDraftPublicId,
      owner.UserPublicId,
      picker.UserPublicId,
      revealer.UserPublicId,
      other.UserPublicId,
      playOrder,
      moviePublicId
    );
  }
}
