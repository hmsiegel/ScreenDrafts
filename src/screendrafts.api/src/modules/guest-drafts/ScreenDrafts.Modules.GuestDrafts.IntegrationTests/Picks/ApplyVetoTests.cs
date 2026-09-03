namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Picks;

public sealed class ApplyVetoTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task ApplyVeto_OnTheMostRecentlyPlayedPick_ShouldSucceedAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, other, _, _) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 7, 1);

    // Act
    var result = await ApplyVetoAsync(guestDraftPublicId, 1, other);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task ApplyVeto_OnAPickThatIsNotTheMostRecentlyPlayed_ShouldFailAsync()
  {
    // Arrange -- the older pick is untouched (unvetoed), but that alone doesn't
    // make it eligible: only the most recent pick, by play order, can be vetoed.
    var (guestDraftPublicId, owner, other, _, _) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 7, 1);
    await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 6, 2);

    // Act
    var result = await ApplyVetoAsync(guestDraftPublicId, 1, other);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.VetoNotOnMostRecentPick.Code);
  }

  [Fact]
  public async Task ApplyVeto_WhenParticipantHasNoRemainingVetoesOrFungibleTokens_ShouldFailAsync()
  {
    // Arrange -- spend the participant's one starting veto, then try again on the
    // (now re-pickable) most recent pick.
    var (guestDraftPublicId, owner, other, _, _) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 7, 1);
    await ApplyVetoAsync(guestDraftPublicId, 1, other);
    await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 7, 2);

    // Act
    var result = await ApplyVetoAsync(guestDraftPublicId, 2, other);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.NoRemainingVetoes.Code);
  }

  [Fact]
  public async Task ApplyVeto_ShouldSpendFromTheFungiblePool_WhenTheNormalPoolIsExhaustedButAFungibleTokenExistsAsync()
  {
    // Arrange
    var (guestDraftPublicId, _, other, _) =
      await CreateDraftWhereOtherHasExhaustedNormalVetoesButHasOneFungibleTokenAsync();

    // Act
    var result = await ApplyVetoAsync(guestDraftPublicId, 2, other);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var otherUserId = (await FakeUsersApi.GetUserByPublicId(other, TestContext.Current.CancellationToken))!.UserId;
    var otherParticipant = guestDraft.Participants.Single(p => p.UserId == otherUserId);
    otherParticipant.VetoesUsed.Should().Be(1, "the normal pool was already exhausted and must stay untouched");
    otherParticipant.FungibleTokensUsed.Should().Be(1);
  }

  [Fact]
  public async Task ApplyVeto_OnAPickThatIsAlreadyVetoed_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, other, _, _) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 7, 1);
    await ApplyVetoAsync(guestDraftPublicId, 1, other);

    // Act -- owner still has their full veto budget; the pick is simply already vetoed
    var result = await ApplyVetoAsync(guestDraftPublicId, 1, owner);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.PickAlreadyVetoed.Code);
  }

  [Fact]
  public async Task ApplyVeto_WhenTheDraftIsNotInProgress_ShouldFailAsync()
  {
    // Arrange -- complete the draft, then try to veto its last (and therefore most
    // recent) pick.
    var (guestDraftPublicId, owner, other, _, _) = await CreateInProgressStandardGuestDraftAsync();
    int[] pickSlots = [7, 6, 4, 2, 5, 3, 1];

    for (var i = 0; i < pickSlots.Length; i++)
    {
      await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), pickSlots[i], i + 1);
    }

    await SetGuestDraftStatusAsync(guestDraftPublicId, owner, GuestDraftStatusAction.Complete);

    // Act
    var result = await ApplyVetoAsync(guestDraftPublicId, 7, other);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.DraftNotStarted.Code);
  }

  /// <summary>
  /// Two-participant MiniMega draft where "other" has already spent their one
  /// starting veto from the normal pool (on a since-re-picked slot) but was
  /// awarded a bonus fungible token, and a fresh, most-recent, un-vetoed pick
  /// (play order 2) is waiting to be vetoed from that fungible pool.
  /// </summary>
  private async Task<(string GuestDraftPublicId, string Owner, string Other, string SecondPickMoviePublicId)>
    CreateDraftWhereOtherHasExhaustedNormalVetoesButHasOneFungibleTokenAsync()
  {
    var owner = CreateUser();
    var other = CreateUser();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner, GuestDraftType.MiniMega);
    await InviteParticipantAsync(guestDraftPublicId, owner, other);

    List<PositionInput> positions =
    [
      new() { Name = "A", Picks = [1] },
      new() { Name = "B", Picks = [2], HasBonusFungibleToken = true },
    ];
    await SetCustomPositionsAsync(guestDraftPublicId, owner, positions);

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var boardPositions = guestDraft.GameBoard!.Positions.ToList();
    var ownerUserId = (await FakeUsersApi.GetUserByPublicId(owner, TestContext.Current.CancellationToken))!.UserId;
    var otherUserId = (await FakeUsersApi.GetUserByPublicId(other, TestContext.Current.CancellationToken))!.UserId;
    var ownerParticipant = guestDraft.Participants.Single(p => p.UserId == ownerUserId);
    var otherParticipant = guestDraft.Participants.Single(p => p.UserId == otherUserId);

    await AssignParticipantAsync(guestDraftPublicId, owner, boardPositions.Single(p => p.Name == "A").PublicId, ownerParticipant.PublicId);
    await AssignParticipantAsync(guestDraftPublicId, owner, boardPositions.Single(p => p.Name == "B").PublicId, otherParticipant.PublicId);
    await SetGuestDraftStatusAsync(guestDraftPublicId, owner, GuestDraftStatusAction.Start);

    await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 1, 1);
    await ApplyVetoAsync(guestDraftPublicId, 1, other);
    var secondMovie = CreateMovie();
    await PlayPickAsync(guestDraftPublicId, owner, secondMovie, 1, 2);

    return (guestDraftPublicId, owner, other, secondMovie);
  }
}
