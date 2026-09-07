namespace ScreenDrafts.Modules.RealTimeUpdates.IntegrationTests.GuestDrafts;

public sealed class GuestDraftPickUndoneConsumerTests
{
  [Fact]
  public async Task Handle_ShouldNotifyTheFlatGuestDraftGroupAsync()
  {
    // Arrange
    var guestDraftPublicId = "gd_test123";
    var integrationEvent = BuildEvent(guestDraftPublicId: guestDraftPublicId);
    var hubContext = new TestHubContext();
    var consumer = new GuestDraftPickUndoneIntegrationEventConsumer(
      NullLogger<GuestDraftPickUndoneIntegrationEventConsumer>.Instance,
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
  public async Task Handle_ShouldSendPickUndoneMethodAsync()
  {
    // Arrange
    var integrationEvent = BuildEvent();
    var hubContext = new TestHubContext();
    var consumer = new GuestDraftPickUndoneIntegrationEventConsumer(
      NullLogger<GuestDraftPickUndoneIntegrationEventConsumer>.Instance,
      hubContext
    );

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Method.Should().Be("PickUndone");
  }

  [Fact]
  public async Task Handle_ShouldPassCorrectArgsInOrderAsync()
  {
    // Arrange
    var guestDraftPublicId = "gd_pub";
    var playOrder = 3;
    var boardPosition = 5;
    var moviePublicId = "m_abc123";

    var integrationEvent = new GuestDraftPickUndoneIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: guestDraftPublicId,
      playOrder: playOrder,
      boardPosition: boardPosition,
      moviePublicId: moviePublicId
    );

    var hubContext = new TestHubContext();
    var consumer = new GuestDraftPickUndoneIntegrationEventConsumer(
      NullLogger<GuestDraftPickUndoneIntegrationEventConsumer>.Instance,
      hubContext
    );

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    var args = hubContext.SentMessages.Single().Args;
    args.Should().HaveCount(4);
    args[0].Should().Be(guestDraftPublicId);
    args[1].Should().Be(playOrder);
    args[2].Should().Be(boardPosition);
    args[3].Should().Be(moviePublicId);
  }

  private static GuestDraftPickUndoneIntegrationEvent BuildEvent(string guestDraftPublicId = "gd_test") =>
    new(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: guestDraftPublicId,
      playOrder: 1,
      boardPosition: 1,
      moviePublicId: "m_test"
    );
}
