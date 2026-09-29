namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.GuestDrafts;

public sealed class SetGuestDraftScheduleTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task SetGuestDraftSchedule_OnACreatedDraft_ShouldSucceedAndPersistAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, DraftType.Standard);
    var scheduledForUtc = new DateTime(2030, 1, 15, 18, 30, 0, DateTimeKind.Utc);

    // Act
    var result = await SetGuestDraftScheduleAsync(
      guestDraftPublicId,
      owner.UserPublicId,
      scheduledForUtc
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    guestDraft.ScheduledForUtc.Should().Be(scheduledForUtc);
  }

  [Fact]
  public async Task SetGuestDraftSchedule_OnAnInProgressDraft_ShouldSucceedAndPersistAsync()
  {
    // Arrange
    var (guestDraftPublicId, owner, _) = await CreateInProgressStandardGuestDraftAsync();
    var scheduledForUtc = new DateTime(2030, 1, 15, 18, 30, 0, DateTimeKind.Utc);

    // Act
    var result = await SetGuestDraftScheduleAsync(guestDraftPublicId, owner, scheduledForUtc);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    guestDraft.ScheduledForUtc.Should().Be(scheduledForUtc);
  }

  [Fact]
  public async Task SetGuestDraftSchedule_WhenAlreadyScheduled_ShouldReplaceTheValueAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(
      owner.UserPublicId,
      DraftType.Standard,
      scheduledForUtc: new DateTime(2030, 1, 15, 18, 30, 0, DateTimeKind.Utc)
    );
    var newScheduledForUtc = new DateTime(2030, 2, 1, 12, 0, 0, DateTimeKind.Utc);

    // Act
    var result = await SetGuestDraftScheduleAsync(
      guestDraftPublicId,
      owner.UserPublicId,
      newScheduledForUtc
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    var guestDraft = await GetGuestDraftWithBoardAsync(guestDraftPublicId);
    guestDraft.ScheduledForUtc.Should().Be(newScheduledForUtc);
  }

  [Fact]
  public async Task SetGuestDraftSchedule_WhenCallerIsNotTheOwner_ShouldFailAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var other = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, DraftType.Standard);

    // Act
    var result = await SetGuestDraftScheduleAsync(
      guestDraftPublicId,
      other.UserPublicId,
      new DateTime(2030, 1, 15, 18, 30, 0, DateTimeKind.Utc)
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DraftErrors.OnlyOwnerCanPerformThisAction.Code);
  }

  [Fact]
  public async Task SetGuestDraftSchedule_ForANonExistentDraft_ShouldFailAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var nonExistentDraftPublicId = $"gd_{Faker.Random.AlphaNumeric(15)}";

    // Act
    var result = await SetGuestDraftScheduleAsync(
      nonExistentDraftPublicId,
      owner.UserPublicId,
      new DateTime(2030, 1, 15, 18, 30, 0, DateTimeKind.Utc)
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result
      .Errors.Should()
      .Contain(e => e.Code == DraftErrors.NotFound(nonExistentDraftPublicId).Code);
  }

  [Fact]
  public async Task SetGuestDraftSchedule_WithNonExistentCaller_ShouldFailAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, DraftType.Standard);
    var nonExistentCaller = $"u_{Faker.Random.AlphaNumeric(15)}";

    // Act
    var result = await SetGuestDraftScheduleAsync(
      guestDraftPublicId,
      nonExistentCaller,
      new DateTime(2030, 1, 15, 18, 30, 0, DateTimeKind.Utc)
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result
      .Errors.Should()
      .Contain(e => e.Code == UserPublicApiErrors.PublicIdNotFound(nonExistentCaller).Code);
  }
}
