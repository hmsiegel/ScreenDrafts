namespace ScreenDrafts.Modules.RealTimeUpdates.IntegrationTests.DraftParts;

public sealed class VetoAppliedConsumerTests
{
  [Fact]
  public async Task Handle_ShouldNotifyCorrectHubGroupAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var integrationEvent = new VetoAppliedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, draftPartId);
    var hubContext = new TestHubContext();
    var consumer = new VetoAppliedIntegrationEventConsumer(
      hubContext,
      NullLogger<VetoAppliedIntegrationEventConsumer>.Instance);

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
    var integrationEvent = new VetoAppliedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid());
    var hubContext = new TestHubContext();
    var consumer = new VetoAppliedIntegrationEventConsumer(
      hubContext,
      NullLogger<VetoAppliedIntegrationEventConsumer>.Instance);

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
    var integrationEvent = new VetoAppliedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, draftPartId);
    var hubContext = new TestHubContext();
    var consumer = new VetoAppliedIntegrationEventConsumer(
      hubContext,
      NullLogger<VetoAppliedIntegrationEventConsumer>.Instance);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Args
      .Should().ContainSingle()
      .Which.Should().Be(draftPartId);
  }
}
