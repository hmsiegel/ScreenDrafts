namespace ScreenDrafts.Modules.RealTimeUpdates.IntegrationTests.DraftParts;

public sealed class PickAddedConsumerTests
{
  // -------------------------------------------------------------------------
  // Group routing
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldNotifyCorrectHubGroupAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var integrationEvent = new PickAddedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, draftPartId, "tt1234567", "Test Movie");
    var hubContext = new TestHubContext();
    var consumer = new PickAddedIntegrationEventConsumer(
      hubContext,
      NullLogger<PickAddedIntegrationEventConsumer>.Instance);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Should().ContainSingle()
      .Which.GroupName.Should().Be(DraftHub.GroupName(draftPartId.ToString()));
  }

  [Fact]
  public async Task Handle_ShouldSendPickListUpdatedMethodAsync()
  {
    // Arrange
    var integrationEvent = new PickAddedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), "tt1234567", "Test Movie");
    var hubContext = new TestHubContext();
    var consumer = new PickAddedIntegrationEventConsumer(
      hubContext,
      NullLogger<PickAddedIntegrationEventConsumer>.Instance);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Method.Should().Be("PickListUpdated");
  }

  [Fact]
  public async Task Handle_ShouldPassDraftPartIdAsArgumentAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var imdbId = "tt1234567";
    var movieTitle = "Test Movie";
    var integrationEvent = new PickAddedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, draftPartId, imdbId, movieTitle);
    var hubContext = new TestHubContext();
    var consumer = new PickAddedIntegrationEventConsumer(
      hubContext,
      NullLogger<PickAddedIntegrationEventConsumer>.Instance);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    var args = hubContext.SentMessages.Single().Args;
    args.Should().HaveCount(3);
    args[0].Should().Be(draftPartId);
    args[1].Should().Be(imdbId);
    args[2].Should().Be(movieTitle);
  }
}
