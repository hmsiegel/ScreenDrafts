namespace ScreenDrafts.Modules.RealTimeUpdates.IntegrationTests.GuestDrafts;

public sealed class GuestDraftCompletedConsumerTests
{
  [Fact]
  public async Task Handle_ShouldNotifyTheFlatGuestDraftGroupAsync()
  {
    // Arrange
    var guestDraftPublicId = "gd_test123";
    var integrationEvent = BuildEvent(guestDraftPublicId: guestDraftPublicId);
    var hubContext = new TestHubContext();
    var consumer = new GuestDraftCompletedIntegrationEventConsumer(
      NullLogger<GuestDraftCompletedIntegrationEventConsumer>.Instance,
      hubContext
    );

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext
      .SentMessages.Should()
      .ContainSingle()
      .Which.GroupName.Should()
      .Be(DraftHub.GuestDraftGroupName(guestDraftPublicId));
  }

  [Fact]
  public async Task Handle_ShouldSendDraftCompletedMethodAsync()
  {
    // Arrange
    var integrationEvent = BuildEvent();
    var hubContext = new TestHubContext();
    var consumer = new GuestDraftCompletedIntegrationEventConsumer(
      NullLogger<GuestDraftCompletedIntegrationEventConsumer>.Instance,
      hubContext
    );

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Method.Should().Be("DraftCompleted");
  }

  [Fact]
  public async Task Handle_ShouldPassCorrectArgsInOrderAsync()
  {
    // Arrange
    var guestDraftPublicId = "gd_pub";
    var totalPicks = 7;
    var vetoCount = 2;

    var integrationEvent = new GuestDraftCompletedIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: guestDraftPublicId,
      totalPicks: totalPicks,
      vetoCount: vetoCount
    );

    var hubContext = new TestHubContext();
    var consumer = new GuestDraftCompletedIntegrationEventConsumer(
      NullLogger<GuestDraftCompletedIntegrationEventConsumer>.Instance,
      hubContext
    );

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    var args = hubContext.SentMessages.Single().Args;
    args.Should().HaveCount(3);
    args[0].Should().Be(guestDraftPublicId);
    args[1].Should().Be(totalPicks);
    args[2].Should().Be(vetoCount);
  }

  private static GuestDraftCompletedIntegrationEvent BuildEvent(string guestDraftPublicId = "gd_test") =>
    new(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: guestDraftPublicId,
      totalPicks: 3,
      vetoCount: 1
    );
}
