namespace ScreenDrafts.Modules.Integrations.IntegrationTests.Zoom;

public sealed class StopZoomRecordingRequestedConsumerTests
{
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldCallStopRecording_WithCorrectSessionNameAsync()
  {
    // Arrange
    using var fakeClient = new FakeZoomApiClient();
    var logger = NullLogger<StopZoomRecordingRequestedIntegrationEventConsumer>.Instance;
    var consumer = new StopZoomRecordingRequestedIntegrationEventConsumer(fakeClient, logger);

    var sessionName = "screendrafts-dp-xyz789";
    var @event = BuildEvent(sessionName, draftPartPublicId: "dp-xyz789");

    // Act
    await consumer.Handle(@event, CancellationToken.None);

    // Assert
    fakeClient.StoppedSessions.Should().ContainSingle()
      .Which.Should().Be(sessionName);
  }

  [Fact]
  public async Task Handle_ShouldNotCallStartRecording_WhenStopEventReceivedAsync()
  {
    // Arrange
    using var fakeClient = new FakeZoomApiClient();
    var consumer = new StopZoomRecordingRequestedIntegrationEventConsumer(
      fakeClient,
      NullLogger<StopZoomRecordingRequestedIntegrationEventConsumer>.Instance);

    // Act
    await consumer.Handle(BuildEvent("screendrafts-dp-test", "dp-test"), CancellationToken.None);

    // Assert
    fakeClient.StartedSessions.Should().BeEmpty();
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private static StopZoomRecordingRequestedIntegrationEvent BuildEvent(
    string sessionName,
    string draftPartPublicId) =>
    new(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      sessionName: sessionName,
      draftPartPublicId: draftPartPublicId);
}
