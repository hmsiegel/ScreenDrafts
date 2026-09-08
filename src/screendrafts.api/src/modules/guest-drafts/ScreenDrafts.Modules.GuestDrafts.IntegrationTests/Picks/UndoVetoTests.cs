using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Enums;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Errors;

namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Picks;

public sealed class UndoVetoTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task UndoVeto_ByTheOwner_ShouldSucceedAndRefundTheNormalPoolAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, other) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, await CreateMovieAsync(), 7, 1);
    await ApplyVetoAsync(guestDraftPublicId, 1, other);

    // Act
    var result = await UndoVetoAsync(guestDraftPublicId, 1, owner);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var otherGuestDrafterId = await GetGuestDrafterIdAsync(other);
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    guestDraft
      .Participants.Single(p => p.ParticipantIdValue == otherGuestDrafterId)
      .VetoesUsed.Should()
      .Be(0);
  }

  [Fact]
  public async Task UndoVeto_WhenCallerIsNotTheOwner_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, other) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, await CreateMovieAsync(), 7, 1);
    await ApplyVetoAsync(guestDraftPublicId, 1, other);

    // Act -- other is a genuine participant, but not the owner
    var result = await UndoVetoAsync(guestDraftPublicId, 1, other);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DraftErrors.OnlyOwnerCanPerformThisAction.Code);
  }

  [Fact]
  public async Task UndoVeto_WhenThePickIsNotVetoed_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, await CreateMovieAsync(), 7, 1);

    // Act
    var result = await UndoVetoAsync(guestDraftPublicId, 1, owner);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DraftErrors.PickNotVetoed.Code);
  }

  [Fact]
  public async Task UndoVeto_WhenTheDraftIsNotInProgress_ShouldFailAsync()
  {
    // Arrange -- still Created, never started
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, other.GuestDrafterPublicId);

    // Act
    var result = await UndoVetoAsync(guestDraftPublicId, 1, owner.UserPublicId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DraftErrors.DraftNotStarted.Code);
  }

  [Fact]
  public async Task UndoVeto_WithANonExistentPlayOrder_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _) = await CreateInProgressStandardGuestDraftAsync();

    // Act
    var result = await UndoVetoAsync(guestDraftPublicId, 99, owner);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DraftErrors.PickNotFoundByPlayOrder(99).Code);
  }

  [Fact]
  public async Task UndoVeto_ByTheOwner_ShouldRefundTheFungiblePool_WhenTheVetoWasSpentFromItAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, other) =
      await CreateDraftWhereOtherHasExhaustedNormalVetoesButHasOneFungibleTokenAsync();

    // Act
    var result = await UndoVetoAsync(guestDraftPublicId, 2, owner.UserPublicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var otherParticipant = guestDraft.Participants.Single(p =>
      p.ParticipantIdValue == other.GuestDrafterId
    );
    otherParticipant.FungibleTokensUsed.Should().Be(0);
    otherParticipant
      .VetoesUsed.Should()
      .Be(1, "only the fungible pool was spent on this veto, the normal pool refund is untouched");
  }

  [Fact]
  public async Task UndoVeto_ShouldNotRefundAnyPool_WhenTheUndoItselfFailsAsync()
  {
    // Arrange -- guards the ordering fix: the refund must only happen after the
    // undo itself has actually succeeded, never unconditionally beforehand.
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();

    List<CreateGuestDraftPositionInput> positions =
    [
      new() { Name = "A", Picks = [1] },
      new()
      {
        Name = "B",
        Picks = [2],
        HasBonusVetoOverride = true,
      },
    ];
    var guestDraftPublicId = await CreateGuestDraftAsync(
      owner.UserPublicId,
      DraftType.MiniMega,
      numberOfPicks: 2,
      positions: positions
    );
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, other.GuestDrafterPublicId);

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var boardPositions = guestDraft.GameBoard!.Positions.ToList();
    await AssignParticipantAsync(
      guestDraftPublicId,
      owner.UserPublicId,
      boardPositions.Single(p => p.Name == "A").PublicId,
      owner.GuestDrafterPublicId
    );
    await AssignParticipantAsync(
      guestDraftPublicId,
      owner.UserPublicId,
      boardPositions.Single(p => p.Name == "B").PublicId,
      other.GuestDrafterPublicId
    );
    await SetGuestDraftStatusAsync(
      guestDraftPublicId,
      owner.UserPublicId,
      GuestDraftStatusAction.Start
    );

    await PlayPickAsync(guestDraftPublicId, owner.UserPublicId, await CreateMovieAsync(), 1, 1);
    await ApplyVetoAsync(guestDraftPublicId, 1, other.UserPublicId);
    await ApplyVetoOverrideAsync(guestDraftPublicId, 1, other.UserPublicId);

    var beforeFailedUndo = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var vetoesUsedBeforeFailedUndo = beforeFailedUndo
      .Participants.Single(p => p.ParticipantIdValue == other.GuestDrafterId)
      .VetoesUsed;

    // Act -- the veto is already overridden, so this must fail
    var result = await UndoVetoAsync(guestDraftPublicId, 1, owner.UserPublicId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result
      .Errors.Should()
      .Contain(e => e.Code == DraftErrors.CannotUndoVetoThatHasBeenOverridden.Code);
    var afterFailedUndo = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    afterFailedUndo
      .Participants.Single(p => p.ParticipantIdValue == other.GuestDrafterId)
      .VetoesUsed.Should()
      .Be(vetoesUsedBeforeFailedUndo, "a failed undo must not refund any pool");
  }

  /// <summary>
  /// Two-participant MiniMega draft where "other" has already spent their one
  /// starting veto from the normal pool (on a since-re-picked slot) but was
  /// awarded a bonus fungible token, and a fresh, most-recent, un-vetoed pick
  /// (play order 2) has since been vetoed from that fungible pool.
  /// </summary>
  private async Task<(
    string GuestDraftPublicId,
    TestUser Owner,
    TestUser Other
  )> CreateDraftWhereOtherHasExhaustedNormalVetoesButHasOneFungibleTokenAsync()
  {
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();

    List<CreateGuestDraftPositionInput> positions =
    [
      new() { Name = "A", Picks = [1] },
      new()
      {
        Name = "B",
        Picks = [2],
        HasBonusFungibleToken = true,
      },
    ];
    var guestDraftPublicId = await CreateGuestDraftAsync(
      owner.UserPublicId,
      DraftType.MiniMega,
      numberOfPicks: 2,
      positions: positions
    );
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, other.GuestDrafterPublicId);

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var boardPositions = guestDraft.GameBoard!.Positions.ToList();

    await AssignParticipantAsync(
      guestDraftPublicId,
      owner.UserPublicId,
      boardPositions.Single(p => p.Name == "A").PublicId,
      owner.GuestDrafterPublicId
    );
    await AssignParticipantAsync(
      guestDraftPublicId,
      owner.UserPublicId,
      boardPositions.Single(p => p.Name == "B").PublicId,
      other.GuestDrafterPublicId
    );
    await SetGuestDraftStatusAsync(
      guestDraftPublicId,
      owner.UserPublicId,
      GuestDraftStatusAction.Start
    );

    await PlayPickAsync(guestDraftPublicId, owner.UserPublicId, await CreateMovieAsync(), 1, 1);
    await ApplyVetoAsync(guestDraftPublicId, 1, other.UserPublicId);
    var secondMovie = await CreateMovieAsync();
    await PlayPickAsync(guestDraftPublicId, owner.UserPublicId, secondMovie, 1, 2);
    await ApplyVetoAsync(guestDraftPublicId, 2, other.UserPublicId);

    return (guestDraftPublicId, owner, other);
  }
}
