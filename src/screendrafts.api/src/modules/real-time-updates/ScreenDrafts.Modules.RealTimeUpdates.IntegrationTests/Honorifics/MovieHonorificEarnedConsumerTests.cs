namespace ScreenDrafts.Modules.RealTimeUpdates.IntegrationTests.Honorifics;

public sealed class MovieHonorificEarnedConsumerTests
{
  // -------------------------------------------------------------------------
  // Group routing
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldNotifyCorrectHubGroupAsync()
  {
    // Arrange
    var draftPartPublicId = "dp_xyz789";
    var integrationEvent = BuildEvent(draftPartPublicId: draftPartPublicId);
    var hubContext = new TestHubContext();
    var consumer = new MovieHonorificEarnedIntegrationEventConsumer(
      hubContext,
      NullLogger<MovieHonorificEarnedIntegrationEventConsumer>.Instance);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Should().ContainSingle()
      .Which.GroupName.Should().Be(DraftHub.GroupName(draftPartPublicId));
  }

  // -------------------------------------------------------------------------
  // Method name
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldSendMovieHonorificEarnedMethodAsync()
  {
    // Arrange
    var integrationEvent = BuildEvent();
    var hubContext = new TestHubContext();
    var consumer = new MovieHonorificEarnedIntegrationEventConsumer(
      hubContext,
      NullLogger<MovieHonorificEarnedIntegrationEventConsumer>.Instance);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Method.Should().Be("MovieHonorificEarned");
  }

  // -------------------------------------------------------------------------
  // Arguments
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldPassCorrectArgumentsAsync()
  {
    // Arrange
    const string moviePublicId = "m_testmovie123";
    const string movieTitle = "The Matrix";
    const int previousAppearanceHonorific = 1;
    const int newAppearanceHonorific = 2;
    const int previousPositionHonorific = 0;
    const int newPositionHonorific = 1;
    const int appearanceCount = 3;

    var integrationEvent = BuildEvent(
      moviePublicId: moviePublicId,
      movieTitle: movieTitle,
      previousAppearanceHonorific: previousAppearanceHonorific,
      newAppearanceHonorific: newAppearanceHonorific,
      previousPositionHonorific: previousPositionHonorific,
      newPositionHonorific: newPositionHonorific,
      appearanceCount: appearanceCount);

    var hubContext = new TestHubContext();
    var consumer = new MovieHonorificEarnedIntegrationEventConsumer(
      hubContext,
      NullLogger<MovieHonorificEarnedIntegrationEventConsumer>.Instance);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    var args = hubContext.SentMessages.Single().Args;
    args.Should().HaveCount(7);
    args[0].Should().Be(moviePublicId);
    args[1].Should().Be(movieTitle);
    args[2].Should().Be(previousAppearanceHonorific);
    args[3].Should().Be(newAppearanceHonorific);
    args[4].Should().Be(previousPositionHonorific);
    args[5].Should().Be(newPositionHonorific);
    args[6].Should().Be(appearanceCount);
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private static MovieHonorificEarnedIntegrationEvent BuildEvent(
    string draftPartPublicId = "dp_test",
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
