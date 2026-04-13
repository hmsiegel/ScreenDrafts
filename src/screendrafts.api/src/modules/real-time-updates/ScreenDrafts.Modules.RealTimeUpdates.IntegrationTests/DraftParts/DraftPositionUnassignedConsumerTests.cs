namespace ScreenDrafts.Modules.RealTimeUpdates.IntegrationTests.DraftParts;

public sealed class DraftPositionUnassignedConsumerTests
{
  [Fact]
  public async Task Handle_ShouldNotifyCorrectHubGroupAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var integrationEvent = new DraftPositionUnassignedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, draftPartId, Guid.NewGuid());
    var hubContext = new TestHubContext();
    var consumer = new DraftPositionUnassignedIntegrationEventConsumer(
      NullLogger<DraftPositionUnassignedIntegrationEventConsumer>.Instance,
      hubContext);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Should().ContainSingle()
      .Which.GroupName.Should().Be(DraftHub.GroupName(draftPartId.ToString()));
  }

  [Fact]
  public async Task Handle_ShouldSendDraftPositionUnassignedMethodAsync()
  {
    // Arrange
    var integrationEvent = new DraftPositionUnassignedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid());
    var hubContext = new TestHubContext();
    var consumer = new DraftPositionUnassignedIntegrationEventConsumer(
      NullLogger<DraftPositionUnassignedIntegrationEventConsumer>.Instance,
      hubContext);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Method.Should().Be("DraftPositionUnassigned");
  }

  [Fact]
  public async Task Handle_ShouldPassCorrectArgumentsAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var draftPositionId = Guid.NewGuid();
    var integrationEvent = new DraftPositionUnassignedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, draftPartId, draftPositionId);
    var hubContext = new TestHubContext();
    var consumer = new DraftPositionUnassignedIntegrationEventConsumer(
      NullLogger<DraftPositionUnassignedIntegrationEventConsumer>.Instance,
      hubContext);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    var args = hubContext.SentMessages.Single().Args;
    args.Should().HaveCount(2);
    args[0].Should().Be(draftPartId);
    args[1].Should().Be(draftPositionId);
  }
}
