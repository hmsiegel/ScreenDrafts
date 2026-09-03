namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Picks;

public sealed class UndoVetoTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task UndoVeto_ByTheOwner_ShouldSucceedAndRefundTheNormalPoolAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, other, _, _) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 7, 1);
    await ApplyVetoAsync(guestDraftPublicId, 1, other);

    // Act
    var result = await UndoVetoAsync(guestDraftPublicId, 1, owner);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var otherUserId = (await FakeUsersApi.GetUserByPublicId(other, TestContext.Current.CancellationToken))!.UserId;
    guestDraft.Participants.Single(p => p.UserId == otherUserId).VetoesUsed.Should().Be(0);
  }

  [Fact]
  public async Task UndoVeto_WhenCallerIsNotTheOwner_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, other, _, _) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 7, 1);
    await ApplyVetoAsync(guestDraftPublicId, 1, other);

    // Act -- other is a genuine participant, but not the owner
    var result = await UndoVetoAsync(guestDraftPublicId, 1, other);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.OnlyOwnerCanPerformThisAction.Code);
  }

  [Fact]
  public async Task UndoVeto_WhenThePickIsNotVetoed_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _, _, _) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 7, 1);

    // Act
    var result = await UndoVetoAsync(guestDraftPublicId, 1, owner);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.PickNotVetoed.Code);
  }

  [Fact]
  public async Task UndoVeto_WhenTheDraftIsNotInProgress_ShouldFailAsync()
  {
    // Arrange -- still Created, never started
    var owner = CreateUser();
    var other = CreateUser();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner);
    await InviteParticipantAsync(guestDraftPublicId, owner, other);

    // Act
    var result = await UndoVetoAsync(guestDraftPublicId, 1, owner);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.DraftNotStarted.Code);
  }

  [Fact]
  public async Task UndoVeto_WithANonExistentPlayOrder_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _, _, _) = await CreateInProgressStandardGuestDraftAsync();

    // Act
    var result = await UndoVetoAsync(guestDraftPublicId, 99, owner);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.PickNotFoundByPlayOrder(99).Code);
  }

  [Fact]
  public async Task UndoVeto_ByTheOwner_ShouldRefundTheFungiblePool_WhenTheVetoWasSpentFromItAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, other, _) =
      await CreateDraftWhereOtherHasExhaustedNormalVetoesButHasOneFungibleTokenAsync();

    // Act
    var result = await UndoVetoAsync(guestDraftPublicId, 2, owner);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var otherUserId = (await FakeUsersApi.GetUserByPublicId(other, TestContext.Current.CancellationToken))!.UserId;
    var otherParticipant = guestDraft.Participants.Single(p => p.UserId == otherUserId);
    otherParticipant.FungibleTokensUsed.Should().Be(0);
    otherParticipant.VetoesUsed.Should().Be(1, "only the fungible pool was spent on this veto, the normal pool refund is untouched");
  }

  [Fact]
  public async Task UndoVeto_ShouldNotRefundAnyPool_WhenTheUndoItselfFailsAsync()
  {
    // Arrange -- guards the ordering fix: the refund must only happen after the
    // undo itself has actually succeeded, never unconditionally beforehand.
    var owner = CreateUser();
    var other = CreateUser();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner, GuestDraftType.MiniMega);
    await InviteParticipantAsync(guestDraftPublicId, owner, other);

    List<PositionInput> positions =
    [
      new() { Name = "A", Picks = [1] },
      new() { Name = "B", Picks = [2], HasBonusVetoOverride = true },
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
    await ApplyVetoOverrideAsync(guestDraftPublicId, 1, other);

    var beforeFailedUndo = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var vetoesUsedBeforeFailedUndo = beforeFailedUndo.Participants.Single(p => p.UserId == otherUserId).VetoesUsed;

    // Act -- the veto is already overridden, so this must fail
    var result = await UndoVetoAsync(guestDraftPublicId, 1, owner);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.CannotUndoVetoThatHasBeenOverridden.Code);
    var afterFailedUndo = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    afterFailedUndo.Participants.Single(p => p.UserId == otherUserId).VetoesUsed
      .Should().Be(vetoesUsedBeforeFailedUndo, "a failed undo must not refund any pool");
  }

  /// <summary>
  /// Two-participant MiniMega draft where "other" has already spent their one
  /// starting veto from the normal pool (on a since-re-picked slot) but was
  /// awarded a bonus fungible token, and a fresh, most-recent, un-vetoed pick
  /// (play order 2) has since been vetoed from that fungible pool.
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
    await ApplyVetoAsync(guestDraftPublicId, 2, other);

    return (guestDraftPublicId, owner, other, secondMovie);
  }
}
