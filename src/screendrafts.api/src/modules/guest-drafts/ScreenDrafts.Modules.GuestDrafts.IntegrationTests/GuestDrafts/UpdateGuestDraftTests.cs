namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.GuestDrafts;

public sealed class UpdateGuestDraftTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task UpdateGuestDraft_Title_ShouldUpdateIndependentlyOfDraftDateAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, DraftType.Standard);

    // Act
    var result = await UpdateGuestDraftAsync(
      guestDraftPublicId,
      owner.UserPublicId,
      title: "New Title"
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    guestDraft.Title.Should().Be("New Title");
    guestDraft.DraftDate.Should().BeNull();
  }

  [Fact]
  public async Task UpdateGuestDraft_DraftDate_ShouldUpdateIndependentlyOfTitleAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(
      owner.UserPublicId,
      DraftType.Standard,
      "Original Title"
    );
    var draftDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

    // Act
    var result = await UpdateGuestDraftAsync(
      guestDraftPublicId,
      owner.UserPublicId,
      draftDate: draftDate
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    guestDraft.DraftDate.Should().Be(draftDate);
    guestDraft.Title.Should().Be("Original Title");
  }

  [Fact]
  public async Task UpdateGuestDraft_DraftDate_ShouldSucceedEvenAfterTheDraftHasStartedAsync()
  {
    // Arrange -- confirmed no status lock on DraftDate
    var (guestDraftPublicId, owner, _) = await CreateInProgressStandardGuestDraftAsync();
    var draftDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

    // Act
    var result = await UpdateGuestDraftAsync(guestDraftPublicId, owner, draftDate: draftDate);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    guestDraft.DraftDate.Should().Be(draftDate);
  }

  [Fact]
  public async Task UpdateGuestDraft_WhenCallerIsNotTheOwner_ShouldFailAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, DraftType.Standard);

    // Act
    var result = await UpdateGuestDraftAsync(
      guestDraftPublicId,
      other.UserPublicId,
      title: "New Title"
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DraftErrors.OnlyOwnerCanPerformThisAction.Code);
  }

  [Fact]
  public async Task UpdateGuestDraft_TypeChangeToADifferentFixedType_ShouldSucceedAndRebuildTheBoardAsync()
  {
    // Arrange -- Standard -> MiniSuper, both fixed
    var owner = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, DraftType.Standard);

    // Act
    var result = await UpdateGuestDraftAsync(
      guestDraftPublicId,
      owner.UserPublicId,
      type: DraftType.MiniSuper
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    guestDraft.GuestDraftType.Should().Be(DraftType.MiniSuper);
    var positions = guestDraft.GameBoard!.Positions;
    positions.Should().HaveCount(2);
    positions.Single(p => p.Name == "A").Picks.Should().BeEquivalentTo([5, 3, 1]);
    positions.Single(p => p.Name == "B").Picks.Should().BeEquivalentTo([4, 2]);
  }

  [Fact]
  public async Task UpdateGuestDraft_TypeChangeToADifferentNonFixedType_WithValidPositions_ShouldSucceedAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(
      owner.UserPublicId,
      DraftType.MiniMega,
      numberOfPicks: 2,
      positions:
      [
        new GuestDraftPositionInput { Name = "A", Picks = [1] },
        new GuestDraftPositionInput { Name = "B", Picks = [2] },
      ]
    );

    // Act
    var result = await UpdateGuestDraftAsync(
      guestDraftPublicId,
      owner.UserPublicId,
      type: DraftType.Super,
      numberOfPicks: 3,
      positions:
      [
        new GuestDraftPositionInput { Name = "X", Picks = [1] },
        new GuestDraftPositionInput { Name = "Y", Picks = [2, 3] },
      ]
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    guestDraft.GuestDraftType.Should().Be(DraftType.Super);
    guestDraft.GameBoard!.Positions.Select(p => p.Name).Should().BeEquivalentTo("X", "Y");
  }

  [Fact]
  public async Task UpdateGuestDraft_TypeChangeToANonFixedType_WithoutPositions_ShouldFailAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, DraftType.Standard);

    // Act -- no NumberOfPicks/Positions supplied for the new non-fixed type
    var result = await UpdateGuestDraftAsync(
      guestDraftPublicId,
      owner.UserPublicId,
      type: DraftType.MiniMega
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result
      .Errors.Should()
      .Contain(e => e.Code == DraftErrors.NumberOfPicksMustBeGreaterThanZero.Code);
  }

  [Fact]
  public async Task UpdateGuestDraft_TypeChangeAfterTheDraftHasStarted_ShouldFailAndLeaveTheBoardUnchangedAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _) = await CreateInProgressStandardGuestDraftAsync();
    var boardBefore = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var positionPublicIdsBefore = boardBefore.GameBoard!.Positions.Select(p => p.PublicId).ToList();

    // Act
    var result = await UpdateGuestDraftAsync(guestDraftPublicId, owner, type: DraftType.Mega);

    // Assert -- fails, and the board must not have been touched at all
    result.IsFailure.Should().BeTrue();
    var boardAfter = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    boardAfter.GuestDraftType.Should().Be(DraftType.Standard);
    boardAfter.GameBoard.Should().NotBeNull();
    boardAfter
      .GameBoard.Positions.Select(p => p.PublicId)
      .Should()
      .BeEquivalentTo(positionPublicIdsBefore);
  }

  [Fact]
  public async Task UpdateGuestDraft_AfterATypeChange_PreviouslyAssignedParticipantsAreNoLongerAssignedToAnyPositionAsync()
  {
    // Arrange -- assign both participants to the Standard fixed layout while still Created
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, DraftType.Standard);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, other.GuestDrafterPublicId);

    var boardBefore = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var positionA = boardBefore.GameBoard!.Positions.Single(p => p.Name == "A");
    var positionB = boardBefore.GameBoard.Positions.Single(p => p.Name == "B");
    await AssignParticipantAsync(
      guestDraftPublicId,
      owner.UserPublicId,
      positionA.PublicId,
      owner.GuestDrafterPublicId
    );
    await AssignParticipantAsync(
      guestDraftPublicId,
      owner.UserPublicId,
      positionB.PublicId,
      other.GuestDrafterPublicId
    );

    // Act -- switch to MiniSuper, whose fixed template also happens to name its
    // positions "A"/"B" -- proving the old assignment doesn't magically carry over
    // to the same-named new position requires checking assignment state directly,
    // not just position names.
    var result = await UpdateGuestDraftAsync(
      guestDraftPublicId,
      owner.UserPublicId,
      type: DraftType.MiniSuper
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    guestDraft.GameBoard!.Positions.Should().OnlyContain(p => p.AssignedToParticipantId == null);
  }
}
