namespace ScreenDrafts.Modules.RealTimeUpdates.UnitTests.Honorifics;

public sealed class MovieHonorificEarnedConsumerTests
{
  // -------------------------------------------------------------------------
  // Handle — broadcasts to the correct SignalR group
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldSendToCorrectGroup_BasedOnDraftPartPublicIdAsync()
  {
    // Arrange
    var hubContext = new TestHubContext();
    var consumer = new MovieHonorificEarnedIntegrationEventConsumer(
      hubContext,
      NullLogger<MovieHonorificEarnedIntegrationEventConsumer>.Instance);

    var draftPartPublicId = "dp-xyz789";
    var integrationEvent = BuildEvent(draftPartPublicId: draftPartPublicId);

    // Act
    await consumer.Handle(integrationEvent);

    // Assert
    hubContext.LastGroupName.Should().Be(DraftHub.GroupName(draftPartPublicId));
  }

  [Fact]
  public async Task Handle_ShouldSendMovieHonorificEarnedMethod_ToGroupAsync()
  {
    // Arrange
    var hubContext = new TestHubContext();
    var consumer = new MovieHonorificEarnedIntegrationEventConsumer(
      hubContext,
      NullLogger<MovieHonorificEarnedIntegrationEventConsumer>.Instance);

    var integrationEvent = BuildEvent();

    // Act
    await consumer.Handle(integrationEvent);

    // Assert
    hubContext.SentMessages.Should().ContainSingle()
      .Which.Method.Should().Be("MovieHonorificEarned");
  }

  [Fact]
  public async Task Handle_ShouldPassCorrectArguments_ToClientAsync()
  {
    // Arrange
    var hubContext = new TestHubContext();
    var consumer = new MovieHonorificEarnedIntegrationEventConsumer(
      hubContext,
      NullLogger<MovieHonorificEarnedIntegrationEventConsumer>.Instance);

    const string moviePublicId = "m_testpublicid";
    const string movieTitle = "The Matrix";

    var integrationEvent = BuildEvent(
      moviePublicId: moviePublicId,
      movieTitle: movieTitle,
      previousAppearanceHonorific: 0,
      newAppearanceHonorific: 1,
      previousPositionHonorific: 0,
      newPositionHonorific: 1,
      appearanceCount: 2);

    // Act
    await consumer.Handle(integrationEvent);

    // Assert
    var (_, args) = hubContext.SentMessages.Single();
    args.Should().HaveCount(7);
    args[0].Should().Be(moviePublicId);
    args[1].Should().Be(movieTitle);
    args[2].Should().Be(0);
    args[3].Should().Be(1);
    args[4].Should().Be(0);
    args[5].Should().Be(1);
    args[6].Should().Be(2);
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private static MovieHonorificEarnedIntegrationEvent BuildEvent(
    string draftPartPublicId = "dp-test",
    string moviePublicId = "m_testid",
    string movieTitle = "Test Movie",
    int previousAppearanceHonorific = 0,
    int newAppearanceHonorific = 1,
    int previousPositionHonorific = 0,
    int newPositionHonorific = 0,
    int appearanceCount = 2)
  {
    return new MovieHonorificEarnedIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      moviePublicId: moviePublicId,
      movieTitle: movieTitle,
      draftPartPublicId: draftPartPublicId,
      previousAppearanceHonorificValue: previousAppearanceHonorific,
      newAppearanceHonorificValue: newAppearanceHonorific,
      previousPositionHonorificValue: previousPositionHonorific,
      newPositionHonorificValue: newPositionHonorific,
      appearanceCount: appearanceCount);
  }
}
