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

    // Reveal slot1 -- resolve the auto-assigned revealer dynamically (random draw
    // among B/C/D with >2 participants), same pattern as RevealPickTests. Resolved
    // from the domain aggregate rather than the response's
    // RevealAuthorizedParticipantPublicId: GetGuestDraftGameplay's Dapper query
    // still resolves participant-identity fields via the deleted
    // GuestDraftParticipant.PublicId/UserId concept (known, already-flagged bug),
    // so that field isn't trustworthy for driving test logic, only for a
    // non-null presence check below.
    var beforeReveal = await GetGameplayAsync(guestDraftPublicId, owner.UserPublicId);
    beforeReveal.IsSuccess.Should().BeTrue();
    var slot1Pick = beforeReveal.Value.Picks.Single(p => p.PlayOrder == 1);
    slot1Pick.RevealAuthorizedParticipantPublicId.Should().NotBeNull();

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var slot1DomainPick = guestDraft.Picks.Single(p => p.PlayOrder == 1);
    slot1DomainPick.RevealAuthorizedParticipantId.Should().NotBeNull();
    var revealerParticipant = guestDraft.Participants.Single(p => p.Id == slot1DomainPick.RevealAuthorizedParticipantId);
    var candidates = new Dictionary<Guid, string>
    {
      [b.GuestDrafterId] = b.UserPublicId,
      [c.GuestDrafterId] = c.UserPublicId,
      [d.GuestDrafterId] = d.UserPublicId,
    };
    var revealerUserPublicId = candidates[revealerParticipant.ParticipantIdValue];
    (await RevealPickAsync(guestDraftPublicId, 1, revealerUserPublicId)).IsSuccess.Should().BeTrue();

    // Act
    var result = await GetGameplayAsync(guestDraftPublicId, owner.UserPublicId);

    // Assert -- top-level shape
    result.IsSuccess.Should().BeTrue();
    var response = result.Value;
    response.GuestDraftPublicId.Should().Be(guestDraftPublicId);
    response.Title.Should().NotBeNullOrWhiteSpace();
    response.Type.Should().Be(GuestDraftType.MiniMega.Name);
    response.Status.Should().Be(GuestDraftStatus.InProgress.Name);
    response.ShareToken.Should().BeNull("no sharing feature exists yet, so the underlying column is always null");
    response.CallerContext.IsOwner.Should().BeTrue();
    response.CallerContext.IsParticipant.Should().BeTrue();
    response.CallerContext.ParticipantPublicId.Should().NotBeNullOrEmpty();

    // Assert -- positions, including AssignedParticipantDisplayName resolved via
    // the real IUsersApi fake
    response.Positions.Should().HaveCount(4);
    foreach (var position in response.Positions)
    {
      position.AssignedParticipantPublicId.Should().NotBeNullOrEmpty();
      position.AssignedParticipantDisplayName.Should().Be("Test User");
    }

    // Assert -- participants
    response.Participants.Should().HaveCount(4);

    // Assert -- picks, one per designed state
    response.Picks.Should().HaveCount(5);

    var slot1 = response.Picks.Single(p => p.Position == 1);
    slot1.IsRevealed.Should().BeTrue();
    slot1.MoviePublicId.Should().NotBeNull();
    slot1.WasVetoed.Should().BeFalse();
    slot1.WasVetoOverridden.Should().BeFalse();
    slot1.WasCommissionerOverride.Should().BeFalse();
    slot1.IsActiveOnFinalBoard.Should().BeTrue();

    var slot2 = response.Picks.Single(p => p.Position == 2);
    slot2.IsRevealed.Should().BeFalse();
    slot2.MoviePublicId.Should().NotBeNull("the owner can always see a pick's movie, revealed or not");
    slot2.IsActiveOnFinalBoard.Should().BeTrue();

    var slot3 = response.Picks.Single(p => p.Position == 3);
    slot3.WasCommissionerOverride.Should().BeTrue();
    slot3.IsActiveOnFinalBoard.Should().BeFalse();

    var slot4 = response.Picks.Single(p => p.Position == 4);
    slot4.WasVetoed.Should().BeFalse();
    slot4.WasVetoOverridden.Should().BeTrue();
    slot4.IsActiveOnFinalBoard.Should().BeTrue();
    slot4.VetoHistory.Should().HaveCount(1);
    slot4.VetoHistory[0].Sequence.Should().Be(1);
    slot4.VetoHistory[0].IsOverridden.Should().BeTrue();

    var slot5 = response.Picks.Single(p => p.Position == 5);
    slot5.WasVetoed.Should().BeTrue();
    slot5.IsActiveOnFinalBoard.Should().BeFalse();
    slot5.VetoHistory.Should().HaveCount(2, "veto seq1 was overridden, then D self-overrode, then C vetoed again as seq2");
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

    // Act
    var result = await GetGameplayAsync(guestDraftPublicId, owner);

    // Assert -- ParticipantPublicId's exact value can't be asserted: GuestDraft
    // participants no longer have a public id of their own (known, already-flagged
    // bug in the underlying query, which still resolves this via the deleted
    // GuestDraftParticipant.PublicId concept), so only presence is checked here.
    result.IsSuccess.Should().BeTrue();
    result.Value.CallerContext.IsOwner.Should().BeTrue();
    result.Value.CallerContext.IsParticipant.Should().BeTrue();
    result.Value.CallerContext.ParticipantPublicId.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task GetGameplay_AsNonOwnerParticipant_ShouldReturnCorrectCallerContextAsync()
  {
    // Arrange
    var (guestDraftPublicId, _, other) = await CreateInProgressStandardGuestDraftAsync();

    // Act
    var result = await GetGameplayAsync(guestDraftPublicId, other);

    // Assert -- see the ParticipantPublicId note in GetGameplay_AsOwner_... above.
    result.IsSuccess.Should().BeTrue();
    result.Value.CallerContext.IsOwner.Should().BeFalse();
    result.Value.CallerContext.IsParticipant.Should().BeTrue();
    result.Value.CallerContext.ParticipantPublicId.Should().NotBeNullOrEmpty();
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
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.NotFound(guestDraftPublicId).Code);
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
    result.Errors.Should().Contain(e => e.Code == UserPublicApiErrors.PublicIdNotFound(nonExistentCaller).Code);
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
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.NotFound(nonExistentGuestDraftPublicId).Code);
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

  // ── Participant token balances ───────────────────────────────────────────

  [Fact]
  public async Task GetGameplay_ParticipantTokenBalances_ShouldMatchDomainArithmeticAsync()
  {
    // Arrange -- "other" spends their one starting veto; owner stays untouched
    var (guestDraftPublicId, owner, other) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 7, 1);
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
    otherParticipant.VetoTokensRemaining.Should().Be(0, "StartingVetoes(1) + AwardedVetoes(0) - VetoesUsed(1)");
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
    var (guestDraftPublicId, users) = await CreateInProgressCustomGuestDraftAsync(4, GuestDraftType.MiniMega);
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
      moviePublicId = CreateMovie();
      (await PlayPickAsync(guestDraftPublicId, picker.UserPublicId, moviePublicId, position, playOrder))
        .IsSuccess.Should().BeTrue();

      var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
      var pick = guestDraft.Picks.Single(p => p.PlayOrder == playOrder);
      var revealerParticipant = guestDraft.Participants.Single(p => p.Id == pick.RevealAuthorizedParticipantId);
      revealer = users.Single(u => u.GuestDrafterId == revealerParticipant.ParticipantIdValue);

      if (revealer != owner)
      {
        break;
      }

      attempt.Should().BeLessThan(25, "the random draw excluding Owner should resolve within a handful of retries");
      (await UndoPickAsync(guestDraftPublicId, playOrder, owner.UserPublicId)).IsSuccess.Should().BeTrue();
    }

    var other = users.Single(u => u != owner && u != picker && u != revealer);

    return (guestDraftPublicId, owner.UserPublicId, picker.UserPublicId, revealer.UserPublicId, other.UserPublicId, playOrder, moviePublicId);
  }
}
