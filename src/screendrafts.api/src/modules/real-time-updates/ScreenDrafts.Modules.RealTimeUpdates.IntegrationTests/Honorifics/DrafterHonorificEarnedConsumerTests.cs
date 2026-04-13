namespace ScreenDrafts.Modules.RealTimeUpdates.IntegrationTests.Honorifics;

public sealed class DrafterHonorificEarnedConsumerTests
{
  // -------------------------------------------------------------------------
  // Group routing
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldNotifyCorrectHubGroupAsync()
  {
    // Arrange
    var draftPartPublicId = "dp_abc123";
    var integrationEvent = BuildEvent(draftPartPublicId: draftPartPublicId);
    var hubContext = new TestHubContext();
    var consumer = new DrafterHonorificEarnedIntegrationEventConsumer(
      hubContext,
      NullLogger<DrafterHonorificEarnedIntegrationEventConsumer>.Instance);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Should().ContainSingle()
      .Which.GroupName.Should().Be(DraftHub.GroupName(draftPartPublicId));
  }

  // -------------------------------------------------------------------------
  // Method name
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldSendDrafterHonorificEarnedMethodAsync()
  {
    // Arrange
    var integrationEvent = BuildEvent();
    var hubContext = new TestHubContext();
    var consumer = new DrafterHonorificEarnedIntegrationEventConsumer(
      hubContext,
      NullLogger<DrafterHonorificEarnedIntegrationEventConsumer>.Instance);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Method.Should().Be("DrafterHonorificEarned");
  }

  // -------------------------------------------------------------------------
  // Arguments
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldPassCorrectArgumentsAsync()
  {
    // Arrange
    var drafterId = Guid.NewGuid();
    const int previousHonorific = 2;
    const int newHonorific = 3;
    const int appearanceCount = 10;

    var integrationEvent = BuildEvent(
      drafterId: drafterId,
      previousHonorific: previousHonorific,
      newHonorific: newHonorific,
      appearanceCount: appearanceCount);

    var hubContext = new TestHubContext();
    var consumer = new DrafterHonorificEarnedIntegrationEventConsumer(
      hubContext,
      NullLogger<DrafterHonorificEarnedIntegrationEventConsumer>.Instance);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    var args = hubContext.SentMessages.Single().Args;
    args.Should().HaveCount(4);
    args[0].Should().Be(drafterId);
    args[1].Should().Be(previousHonorific);
    args[2].Should().Be(newHonorific);
    args[3].Should().Be(appearanceCount);
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private static DrafterHonorificEarnedIntegrationEvent BuildEvent(
    string draftPartPublicId = "dp_test",
    Guid? drafterId = null,
    int previousHonorific = 1,
    int newHonorific = 2,
    int appearanceCount = 5)
  {
    return new DrafterHonorificEarnedIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      drafterIdValue: drafterId ?? Guid.NewGuid(),
      draftPartPublicId: draftPartPublicId,
      previousHonorificValue: previousHonorific,
      newHonorificValue: newHonorific,
      appearanceCount: appearanceCount);
  }
}
