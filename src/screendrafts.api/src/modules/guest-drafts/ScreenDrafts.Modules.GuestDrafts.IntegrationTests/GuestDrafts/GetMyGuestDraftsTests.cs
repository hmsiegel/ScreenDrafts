namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.GuestDrafts;

public sealed class GetMyGuestDraftsTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task GetMyDrafts_ShouldIncludeADraftTheCallerOwnsAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, DraftType.Standard);

    // Act
    var result = await GetMyDraftsAsync(owner.UserPublicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Upcoming.Should().Contain(i => i.PublicId == guestDraftPublicId && i.IsOwner);
  }

  [Fact]
  public async Task GetMyDrafts_ShouldIncludeADraftWhereTheCallerIsOnlyAParticipantAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var participant = await CreateUserAsync();
    var guestDraftPublicId = await CreateGuestDraftAsync(owner.UserPublicId, DraftType.Standard);
    (
      await AddParticipantAsync(
        guestDraftPublicId,
        owner.UserPublicId,
        participant.GuestDrafterPublicId
      )
    )
      .IsSuccess.Should()
      .BeTrue();

    // Act
    var result = await GetMyDraftsAsync(participant.UserPublicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result
      .Value.Upcoming.Should()
      .Contain(i => i.PublicId == guestDraftPublicId && !i.IsOwner);
  }

  [Fact]
  public async Task GetMyDrafts_ACreatedDraft_ShouldLandInUpcomingAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var createdPublicId = await CreateGuestDraftAsync(owner.UserPublicId, DraftType.Standard);

    // Act
    var result = await GetMyDraftsAsync(owner.UserPublicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Upcoming.Should().Contain(i => i.PublicId == createdPublicId);
    result.Value.InProgress.Should().NotContain(i => i.PublicId == createdPublicId);
  }

  [Fact]
  public async Task GetMyDrafts_AnInProgressDraft_ShouldLandInInProgressAsync()
  {
    // Arrange
    var (guestDraftPublicId, ownerUserPublicId, _) =
      await CreateInProgressStandardGuestDraftAsync();

    // Act
    var result = await GetMyDraftsAsync(ownerUserPublicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.InProgress.Should().Contain(i => i.PublicId == guestDraftPublicId);
    result.Value.Upcoming.Should().NotContain(i => i.PublicId == guestDraftPublicId);
  }

  [Fact]
  public async Task GetMyDrafts_ScheduledForUtc_ShouldRoundTripAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var scheduledForUtc = new DateTime(2030, 1, 15, 18, 30, 0, DateTimeKind.Utc);
    var guestDraftPublicId = await CreateGuestDraftAsync(
      owner.UserPublicId,
      DraftType.Standard,
      scheduledForUtc: scheduledForUtc
    );

    // Act
    var result = await GetMyDraftsAsync(owner.UserPublicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result
      .Value.Upcoming.Single(i => i.PublicId == guestDraftPublicId)
      .ScheduledForUtc.Should()
      .Be(scheduledForUtc);
  }

  [Fact]
  public async Task GetMyDrafts_ShouldNeverIncludeAnotherUsersDraftAsync()
  {
    // Arrange
    var owner = await CreateUserAsync();
    var stranger = await CreateUserAsync();
    await CreateGuestDraftAsync(owner.UserPublicId, DraftType.Standard);

    // Act
    var result = await GetMyDraftsAsync(stranger.UserPublicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Upcoming.Should().BeEmpty();
    result.Value.InProgress.Should().BeEmpty();
    result.Value.Completed.Should().BeEmpty();
  }
}
