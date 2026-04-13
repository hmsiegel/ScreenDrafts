namespace ScreenDrafts.Modules.RealTimeUpdates.IntegrationTests.DraftParts;

public sealed class DraftPositionAssignedConsumerTests
{
  [Fact]
  public async Task Handle_ShouldNotifyCorrectHubGroupAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var integrationEvent = new DraftPositionAssignedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, draftPartId, Guid.NewGuid(), Guid.NewGuid(), 1);
    var hubContext = new TestHubContext();
    var consumer = new DraftPositionAssignedIntegrationEventConsumer(
      NullLogger<DraftPositionAssignedIntegrationEventConsumer>.Instance,
      hubContext);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Should().ContainSingle()
      .Which.GroupName.Should().Be(DraftHub.GroupName(draftPartId.ToString()));
  }

  [Fact]
  public async Task Handle_ShouldSendDraftPositionAssignedMethodAsync()
  {
    // Arrange
    var integrationEvent = new DraftPositionAssignedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1);
    var hubContext = new TestHubContext();
    var consumer = new DraftPositionAssignedIntegrationEventConsumer(
      NullLogger<DraftPositionAssignedIntegrationEventConsumer>.Instance,
      hubContext);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Method.Should().Be("DraftPositionAssigned");
  }

  [Fact]
  public async Task Handle_ShouldPassCorrectArgumentsAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var draftPositionId = Guid.NewGuid();
    var participantId = Guid.NewGuid();
    const int participantKind = 1;
    var integrationEvent = new DraftPositionAssignedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, draftPartId, draftPositionId, participantId, participantKind);
    var hubContext = new TestHubContext();
    var consumer = new DraftPositionAssignedIntegrationEventConsumer(
      NullLogger<DraftPositionAssignedIntegrationEventConsumer>.Instance,
      hubContext);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    var args = hubContext.SentMessages.Single().Args;
    args.Should().HaveCount(4);
    args[0].Should().Be(draftPartId);
    args[1].Should().Be(draftPositionId);
    args[2].Should().Be(participantId);
    args[3].Should().Be(participantKind);
  }
}
