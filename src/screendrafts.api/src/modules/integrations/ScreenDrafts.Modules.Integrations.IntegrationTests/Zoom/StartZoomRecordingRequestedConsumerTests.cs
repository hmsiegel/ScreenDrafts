namespace ScreenDrafts.Modules.Integrations.IntegrationTests.Zoom;

public sealed class StartZoomRecordingRequestedConsumerTests
{
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldCallStartRecording_WithCorrectSessionNameAsync()
  {
    // Arrange
    using var fakeClient = new FakeZoomApiClient();
    var logger = NullLogger<StartZoomRecordingRequestedIntegrationEventConsumer>.Instance;
    var consumer = new StartZoomRecordingRequestedIntegrationEventConsumer(fakeClient, logger);

    var sessionName = "screendrafts-dp-abc123";
    var @event = BuildEvent(sessionName, draftPartPublicId: "dp-abc123");

    // Act
    await consumer.Handle(@event, CancellationToken.None);

    // Assert
    fakeClient.StartedSessions.Should().ContainSingle()
      .Which.Should().Be(sessionName);
  }

  [Fact]
  public async Task Handle_ShouldNotCallStopRecording_WhenStartEventReceivedAsync()
  {
    // Arrange
    using var fakeClient = new FakeZoomApiClient();
    var consumer = new StartZoomRecordingRequestedIntegrationEventConsumer(
      fakeClient,
      NullLogger<StartZoomRecordingRequestedIntegrationEventConsumer>.Instance);

    // Act
    await consumer.Handle(BuildEvent("screendrafts-dp-test", "dp-test"), CancellationToken.None);

    // Assert
    fakeClient.StoppedSessions.Should().BeEmpty();
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private static StartZoomRecordingRequestedIntegrationEvent BuildEvent(
    string sessionName,
    string draftPartPublicId) =>
    new(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      sessionName: sessionName,
      draftPartPublicId: draftPartPublicId);
}
