namespace ScreenDrafts.Modules.RealTimeUpdates.IntegrationTests.DraftParts;

public sealed class VetoAppliedConsumerTests
{
  [Fact]
  public async Task Handle_ShouldNotifyCorrectHubGroupAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var draftPartPublicId = "dp_test123";
    var integrationEvent = new VetoAppliedIntegrationEvent(
      Guid.NewGuid(), DateTime.UtcNow, draftPartId, draftPartPublicId,
      1, 12345, "Test Movie", Guid.NewGuid(), 1, Guid.NewGuid(), 1);
    var hubContext = new TestHubContext();
    var fakeDb = new FakeDbConnectionFactory();
    fakeDb.EnqueueEmptyResult();
    var consumer = new VetoAppliedIntegrationEventConsumer(
      hubContext,
      NullLogger<VetoAppliedIntegrationEventConsumer>.Instance,
      fakeDb);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Should().ContainSingle()
      .Which.GroupName.Should().Be(DraftHub.GroupName(draftPartPublicId));
  }

  [Fact]
  public async Task Handle_ShouldSendVetoAppliedMethodAsync()
  {
    // Arrange
    var integrationEvent = new VetoAppliedIntegrationEvent(
      Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), "dp_test123",
      1, 12345, "Test Movie", Guid.NewGuid(), 1, Guid.NewGuid(), 1);
    var hubContext = new TestHubContext();
    var fakeDb = new FakeDbConnectionFactory();
    fakeDb.EnqueueEmptyResult();
    var consumer = new VetoAppliedIntegrationEventConsumer(
      hubContext,
      NullLogger<VetoAppliedIntegrationEventConsumer>.Instance,
      fakeDb);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Method.Should().Be("VetoApplied");
  }

  [Fact]
  public async Task Handle_ShouldSendPayloadWithDraftPartPublicIdAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var draftPartPublicId = "dp_test123";
    var integrationEvent = new VetoAppliedIntegrationEvent(
      Guid.NewGuid(), DateTime.UtcNow, draftPartId, draftPartPublicId,
      1, 12345, "Test Movie", Guid.NewGuid(), 1, Guid.NewGuid(), 1);
    var hubContext = new TestHubContext();
    var fakeDb = new FakeDbConnectionFactory();
    fakeDb.EnqueueEmptyResult();
    var consumer = new VetoAppliedIntegrationEventConsumer(
      hubContext,
      NullLogger<VetoAppliedIntegrationEventConsumer>.Instance,
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
