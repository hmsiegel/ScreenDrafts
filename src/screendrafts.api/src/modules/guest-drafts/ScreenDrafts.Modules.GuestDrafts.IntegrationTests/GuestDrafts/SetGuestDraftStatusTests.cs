namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.GuestDrafts;

public sealed class SetGuestDraftStatusTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  // ── Start ────────────────────────────────────────────────────────────────

  [Fact]
  public async Task Start_WithValidBoardAndParticipants_ShouldSucceedAndReturnInProgressStatusAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, GuestDraftType.Standard);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, other.GuestDrafterPublicId);

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var positions = guestDraft.GameBoard!.Positions.ToList();
    await AssignParticipantAsync(guestDraftPublicId, owner.UserPublicId, positions[0].PublicId, owner.GuestDrafterPublicId);
    await AssignParticipantAsync(guestDraftPublicId, owner.UserPublicId, positions[1].PublicId, other.GuestDrafterPublicId);

    // Act
    var result = await SetGuestDraftStatusAsync(guestDraftPublicId, owner.UserPublicId, GuestDraftStatusAction.Start);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.GuestDraftPublicId.Should().Be(guestDraftPublicId);
    result.Value.Status.Should().Be(GuestDraftStatus.InProgress.Name);
  }

  [Fact]
  public async Task Start_WhenCallerIsNotTheOwner_ShouldFailAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, other.GuestDrafterPublicId);

    // Act
    var result = await SetGuestDraftStatusAsync(guestDraftPublicId, other.UserPublicId, GuestDraftStatusAction.Start);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.OnlyOwnerCanPerformThisAction.Code);
  }

  [Fact]
  public async Task Start_WithFewerThanTwoParticipants_ShouldFailAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId);

    // Act
    var result = await SetGuestDraftStatusAsync(guestDraftPublicId, owner.UserPublicId, GuestDraftStatusAction.Start);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.CannotStartWithoutAtLeastTwoParticipants.Code);
  }

  [Fact]
  public async Task Start_WhenPositionCountDoesNotMatchParticipantCount_ShouldFailAsync()
  {
    // Arrange -- Standard's fixed layout always has 2 positions, but a 3rd
    // participant is added after Create already established the board. Create
    // now always establishes a board on success (fixed template or valid custom
    // positions), so a null-board scenario is no longer reachable -- a count
    // mismatch is the only remaining path to this error.
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();
    var third = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, GuestDraftType.Standard);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, other.GuestDrafterPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, third.GuestDrafterPublicId);

    // Act
    var result = await SetGuestDraftStatusAsync(guestDraftPublicId, owner.UserPublicId, GuestDraftStatusAction.Start);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.BoardMustBeFullySetUpBeforeStarting.Code);
  }

  [Fact]
  public async Task Start_WhenNotAllPositionsAreAssigned_ShouldFailAsync()
  {
    // Arrange -- board exists (Create auto-applied the fixed layout) and
    // participant count matches, but nobody has been assigned to a position yet.
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, GuestDraftType.Standard);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, other.GuestDrafterPublicId);

    // Act
    var result = await SetGuestDraftStatusAsync(guestDraftPublicId, owner.UserPublicId, GuestDraftStatusAction.Start);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.AllPositionsMustBeAssignedBeforeStarting.Code);
  }

  // ── Complete ─────────────────────────────────────────────────────────────

  [Fact]
  public async Task Complete_AfterEveryPositionHasLanded_ShouldSucceedAndReturnCompletedStatusAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _) = await CreateInProgressStandardGuestDraftAsync();
    int[] pickSlots = [7, 6, 4, 2, 5, 3, 1];

    for (var i = 0; i < pickSlots.Length; i++)
    {
      var movie = CreateMovie();
      (await PlayPickAsync(guestDraftPublicId, owner, movie, pickSlots[i], i + 1)).IsSuccess
        .Should().BeTrue("test setup must be able to play every pick");
    }

    // Act
    var result = await SetGuestDraftStatusAsync(guestDraftPublicId, owner, GuestDraftStatusAction.Complete);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.GuestDraftPublicId.Should().Be(guestDraftPublicId);
    result.Value.Status.Should().Be(GuestDraftStatus.Completed.Name);
  }

  [Fact]
  public async Task Complete_BeforeEveryPositionHasLanded_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _) = await CreateInProgressStandardGuestDraftAsync();
    await PlayPickAsync(guestDraftPublicId, owner, CreateMovie(), 7, 1);

    // Act
    var result = await SetGuestDraftStatusAsync(guestDraftPublicId, owner, GuestDraftStatusAction.Complete);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.CannotCompleteWithoutAllPicks.Code);
  }

  [Fact]
  public async Task Complete_WhenCallerIsNotTheOwner_ShouldFailAsync()
  {
    // Arrange
    var (guestDraftPublicId, _, other) = await CreateInProgressStandardGuestDraftAsync();

    // Act
    var result = await SetGuestDraftStatusAsync(guestDraftPublicId, other, GuestDraftStatusAction.Complete);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.OnlyOwnerCanPerformThisAction.Code);
  }

  [Fact]
  public async Task Complete_WhenStatusIsNotInProgress_ShouldFailAsync()
  {
    // Arrange -- still Created, never started
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, owner.GuestDrafterPublicId);
    await AddParticipantAsync(guestDraftPublicId, owner.UserPublicId, other.GuestDrafterPublicId);

    // Act
    var result = await SetGuestDraftStatusAsync(guestDraftPublicId, owner.UserPublicId, GuestDraftStatusAction.Complete);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.CannotCompleteIfNotInProgress.Code);
  }
}
