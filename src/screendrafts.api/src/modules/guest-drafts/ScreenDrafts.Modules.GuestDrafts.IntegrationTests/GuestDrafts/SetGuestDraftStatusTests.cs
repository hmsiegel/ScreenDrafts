namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.GuestDrafts;

public sealed class SetGuestDraftStatusTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  // ── Start ────────────────────────────────────────────────────────────────

  [Fact]
  public async Task Start_WithValidBoardAndParticipants_ShouldSucceedAndReturnInProgressStatusAsync()
  {
    // Arrange
    var owner = CreateUser();
    var other = CreateUser();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner, GuestDraftType.Standard);
    await InviteParticipantAsync(guestDraftPublicId, owner, other);
    await SetFixedBoardLayoutAsync(guestDraftPublicId, owner);

    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    var positions = guestDraft.GameBoard!.Positions.ToList();
    var ownerUserId = (await FakeUsersApi.GetUserByPublicId(owner, TestContext.Current.CancellationToken))!.UserId;
    var otherUserId = (await FakeUsersApi.GetUserByPublicId(other, TestContext.Current.CancellationToken))!.UserId;
    var ownerParticipant = guestDraft.Participants.Single(p => p.UserId == ownerUserId);
    var otherParticipant = guestDraft.Participants.Single(p => p.UserId == otherUserId);
    await AssignParticipantAsync(guestDraftPublicId, owner, positions[0].PublicId, ownerParticipant.PublicId);
    await AssignParticipantAsync(guestDraftPublicId, owner, positions[1].PublicId, otherParticipant.PublicId);

    // Act
    var result = await SetGuestDraftStatusAsync(guestDraftPublicId, owner, GuestDraftStatusAction.Start);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.GuestDraftPublicId.Should().Be(guestDraftPublicId);
    result.Value.Status.Should().Be(GuestDraftStatus.InProgress.Name);
  }

  [Fact]
  public async Task Start_WhenCallerIsNotTheOwner_ShouldFailAsync()
  {
    // Arrange
    var owner = CreateUser();
    var other = CreateUser();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner);
    await InviteParticipantAsync(guestDraftPublicId, owner, other);
    await SetFixedBoardLayoutAsync(guestDraftPublicId, owner);

    // Act
    var result = await SetGuestDraftStatusAsync(guestDraftPublicId, other, GuestDraftStatusAction.Start);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.OnlyOwnerCanPerformThisAction.Code);
  }

  [Fact]
  public async Task Start_WithFewerThanTwoParticipants_ShouldFailAsync()
  {
    // Arrange
    var owner = CreateUser();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner);

    // Act
    var result = await SetGuestDraftStatusAsync(guestDraftPublicId, owner, GuestDraftStatusAction.Start);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.CannotStartWithoutAtLeastTwoParticipants.Code);
  }

  [Fact]
  public async Task Start_WhenBoardHasNotBeenSetUp_ShouldFailAsync()
  {
    // Arrange
    var owner = CreateUser();
    var other = CreateUser();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner);
    await InviteParticipantAsync(guestDraftPublicId, owner, other);

    // Act
    var result = await SetGuestDraftStatusAsync(guestDraftPublicId, owner, GuestDraftStatusAction.Start);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.BoardMustBeFullySetUpBeforeStarting.Code);
  }

  // ── Complete ─────────────────────────────────────────────────────────────

  [Fact]
  public async Task Complete_AfterEveryPositionHasLanded_ShouldSucceedAndReturnCompletedStatusAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _, _, _) = await CreateInProgressStandardGuestDraftAsync();
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
    var (guestDraftPublicId, owner, _, _, _) = await CreateInProgressStandardGuestDraftAsync();
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
    var (guestDraftPublicId, _, other, _, _) = await CreateInProgressStandardGuestDraftAsync();

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
    var owner = CreateUser();
    var other = CreateUser();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner);
    await InviteParticipantAsync(guestDraftPublicId, owner, other);

    // Act
    var result = await SetGuestDraftStatusAsync(guestDraftPublicId, owner, GuestDraftStatusAction.Complete);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == GuestDraftErrors.CannotCompleteIfNotInProgress.Code);
  }
}
