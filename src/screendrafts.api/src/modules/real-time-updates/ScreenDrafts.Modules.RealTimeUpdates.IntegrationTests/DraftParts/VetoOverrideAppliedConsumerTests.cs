namespace ScreenDrafts.Modules.RealTimeUpdates.IntegrationTests.DraftParts;

public sealed class VetoOverrideAppliedConsumerTests
{
  [Fact]
  public async Task Handle_ShouldNotifyCorrectHubGroupAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var draftPartPublicId = "dp_test456";
    var integrationEvent = new VetoOverrideAppliedIntegrationEvent(
      Guid.NewGuid(), DateTime.UtcNow, draftPartId, draftPartPublicId,
      1, 12345, "Test Movie", Guid.NewGuid(), 1);
    var hubContext = new TestHubContext();
    var fakeDb = new FakeDbConnectionFactory();
    fakeDb.EnqueueEmptyResult();
    var consumer = new VetoOverrideAppliedIntegrationEventConsumer(
      hubContext,
      NullLogger<VetoOverrideAppliedIntegrationEventConsumer>.Instance,
      fakeDb);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Should().ContainSingle()
      .Which.GroupName.Should().Be(DraftHub.GroupName(draftPartPublicId));
  }

  [Fact]
  public async Task Handle_ShouldSendVetoOverrideAppliedMethodAsync()
  {
    // Arrange
    var integrationEvent = new VetoOverrideAppliedIntegrationEvent(
      Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), "dp_test456",
      1, 12345, "Test Movie", Guid.NewGuid(), 1);
    var hubContext = new TestHubContext();
    var fakeDb = new FakeDbConnectionFactory();
    fakeDb.EnqueueEmptyResult();
    var consumer = new VetoOverrideAppliedIntegrationEventConsumer(
      hubContext,
      NullLogger<VetoOverrideAppliedIntegrationEventConsumer>.Instance,
      fakeDb);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Method.Should().Be("VetoOverrideApplied");
  }

  [Fact]
  public async Task Handle_ShouldSendPayloadWithDraftPartPublicIdAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var draftPartPublicId = "dp_test456";
    var integrationEvent = new VetoOverrideAppliedIntegrationEvent(
      Guid.NewGuid(), DateTime.UtcNow, draftPartId, draftPartPublicId,
      1, 12345, "Test Movie", Guid.NewGuid(), 1);
    var hubContext = new TestHubContext();
    var fakeDb = new FakeDbConnectionFactory();
    fakeDb.EnqueueEmptyResult();
    var consumer = new VetoOverrideAppliedIntegrationEventConsumer(
      hubContext,
      NullLogger<VetoOverrideAppliedIntegrationEventConsumer>.Instance,
      fakeDb);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    var args = hubContext.SentMessages.Single().Args;
    args.Should().ContainSingle();
    dynamic payload = args[0]!;
    ((string)payload.DraftPartPublicId).Should().Be(draftPartPublicId);
  }
}
