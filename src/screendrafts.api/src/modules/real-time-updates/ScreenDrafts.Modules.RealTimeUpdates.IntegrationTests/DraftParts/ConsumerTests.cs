namespace ScreenDrafts.Modules.RealTimeUpdates.IntegrationTests.DraftParts;

public sealed class PickAddedConsumerTests
{
  [Fact]
  public async Task Handle_ShouldNotifyCorrectHubGroupAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var draftPartPublicId = "dp_pickadd123";
    var integrationEvent = new PickAddedIntegrationEvent(
      Guid.NewGuid(), DateTime.UtcNow, draftPartId, draftPartPublicId,
      "tt1234567", "Test Movie", 99999, 1, 1, Guid.NewGuid(), 1);
    var hubContext = new TestHubContext();
    var fakeDb = new FakeDbConnectionFactory();
    fakeDb.EnqueueEmptyResult();
    var consumer = new PickAddedIntegrationEventConsumer(
      hubContext,
      NullLogger<PickAddedIntegrationEventConsumer>.Instance,
      fakeDb);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Should().ContainSingle()
      .Which.GroupName.Should().Be(DraftHub.GroupName(draftPartPublicId));
  }

  [Fact]
  public async Task Handle_ShouldSendPickAddedMethodAsync()
  {
    // Arrange
    var integrationEvent = new PickAddedIntegrationEvent(
      Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), "dp_pickadd123",
      "tt1234567", "Test Movie", 99999, 1, 1, Guid.NewGuid(), 1);
    var hubContext = new TestHubContext();
    var fakeDb = new FakeDbConnectionFactory();
    fakeDb.EnqueueEmptyResult();
    var consumer = new PickAddedIntegrationEventConsumer(
      hubContext,
      NullLogger<PickAddedIntegrationEventConsumer>.Instance,
      fakeDb);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Method.Should().Be("PickAdded");
  }

  [Fact]
  public async Task Handle_ShouldSendPayloadWithDraftPartPublicIdAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var draftPartPublicId = "dp_pickadd123";
    var imdbId = "tt1234567";
    var movieTitle = "Test Movie";
    var integrationEvent = new PickAddedIntegrationEvent(
      Guid.NewGuid(), DateTime.UtcNow, draftPartId, draftPartPublicId,
      imdbId, movieTitle, 99999, 1, 1, Guid.NewGuid(), 1);
    var hubContext = new TestHubContext();
    var fakeDb = new FakeDbConnectionFactory();
    fakeDb.EnqueueEmptyResult();
    var consumer = new PickAddedIntegrationEventConsumer(
      hubContext,
      NullLogger<PickAddedIntegrationEventConsumer>.Instance,
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
