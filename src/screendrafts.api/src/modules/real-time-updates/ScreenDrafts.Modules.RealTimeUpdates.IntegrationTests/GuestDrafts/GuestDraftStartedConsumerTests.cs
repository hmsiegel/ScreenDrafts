namespace ScreenDrafts.Modules.RealTimeUpdates.IntegrationTests.GuestDrafts;

public sealed class GuestDraftStartedConsumerTests
{
  [Fact]
  public async Task Handle_ShouldNotifyTheFlatGuestDraftGroupAsync()
  {
    // Arrange
    var guestDraftPublicId = "gd_test123";
    var integrationEvent = BuildEvent(guestDraftPublicId: guestDraftPublicId);
    var hubContext = new TestHubContext();
    var consumer = new GuestDraftStartedIntegrationEventConsumer(
      NullLogger<GuestDraftStartedIntegrationEventConsumer>.Instance,
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
  public async Task Handle_ShouldSendDraftStartedMethodAsync()
  {
    // Arrange
    var integrationEvent = BuildEvent();
    var hubContext = new TestHubContext();
    var consumer = new GuestDraftStartedIntegrationEventConsumer(
      NullLogger<GuestDraftStartedIntegrationEventConsumer>.Instance,
      hubContext
    );

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Method.Should().Be("DraftStarted");
  }

  [Fact]
  public async Task Handle_ShouldPassCorrectArgsInOrderAsync()
  {
    // Arrange
    var guestDraftPublicId = "gd_pub";
    var participantCount = 4;

    var integrationEvent = new GuestDraftStartedIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: guestDraftPublicId,
      participantCount: participantCount
    );

    var hubContext = new TestHubContext();
    var consumer = new GuestDraftStartedIntegrationEventConsumer(
      NullLogger<GuestDraftStartedIntegrationEventConsumer>.Instance,
      hubContext
    );

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    var args = hubContext.SentMessages.Single().Args;
    args.Should().HaveCount(2);
    args[0].Should().Be(guestDraftPublicId);
    args[1].Should().Be(participantCount);
  }

  private static GuestDraftStartedIntegrationEvent BuildEvent(string guestDraftPublicId = "gd_test") =>
    new(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: guestDraftPublicId,
      participantCount: 2
    );
}
