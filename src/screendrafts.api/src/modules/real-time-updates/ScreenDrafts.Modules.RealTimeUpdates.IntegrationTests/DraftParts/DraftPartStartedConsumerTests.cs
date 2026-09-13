namespace ScreenDrafts.Modules.RealTimeUpdates.IntegrationTests.DraftParts;

public sealed class DraftPartStartedConsumerTests
{
  private static DraftPartStartedIntegrationEvent BuildEvent(string draftPartPublicId = "dp_started123") =>
    new(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      draftPartId: Guid.NewGuid(),
      draftPartPublicId: draftPartPublicId,
      draftId: Guid.NewGuid(),
      draftPublicId: "d_started123",
      partIndex: 1,
      participants: [],
      canonicalPolicyValue: 0,
      hasMainFeedRelease: false
    );

  [Fact]
  public async Task Handle_ShouldBroadcastDraftPartStartedToTheCorrectGroupAsync()
  {
    // Arrange
    var draftPartPublicId = "dp_started123";
    var hubContext = new TestHubContext();
    var consumer = new DraftPartStartedIntegrationEventConsumer(
      hubContext,
      NullLogger<DraftPartStartedIntegrationEventConsumer>.Instance
    );

    // Act
    await consumer.Handle(BuildEvent(draftPartPublicId), CancellationToken.None);

    // Assert
    hubContext
      .SentMessages.Should()
      .ContainSingle()
      .Which.GroupName.Should()
      .Be(DraftHub.GroupName(draftPartPublicId));
    hubContext.SentMessages.Single().Method.Should().Be("DraftPartStarted");
  }

  [Fact]
  public async Task Handle_ShouldSendPayloadWithDraftPartAndPartIndexAsync()
  {
    // Arrange
    var draftPartPublicId = "dp_started123";
    var hubContext = new TestHubContext();
    var consumer = new DraftPartStartedIntegrationEventConsumer(
      hubContext,
      NullLogger<DraftPartStartedIntegrationEventConsumer>.Instance
    );
    var integrationEvent = BuildEvent(draftPartPublicId);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    dynamic payload = hubContext.SentMessages.Single().Args[0]!;
    ((string)payload.DraftPartPublicId).Should().Be(draftPartPublicId);
    ((int)payload.PartIndex).Should().Be(integrationEvent.PartIndex);
  }
}
