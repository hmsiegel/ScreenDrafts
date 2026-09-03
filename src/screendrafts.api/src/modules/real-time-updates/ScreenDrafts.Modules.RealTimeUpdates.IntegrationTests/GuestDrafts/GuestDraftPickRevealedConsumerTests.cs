namespace ScreenDrafts.Modules.RealTimeUpdates.IntegrationTests.GuestDrafts;

public sealed class GuestDraftPickRevealedConsumerTests
{
  [Fact]
  public async Task Handle_ShouldNotifyTheFlatGuestDraftGroupAsync()
  {
    // Arrange
    var guestDraftPublicId = "gd_test123";
    var integrationEvent = BuildEvent(guestDraftPublicId: guestDraftPublicId);
    var hubContext = new TestHubContext();
    var consumer = new GuestDraftPickRevealedIntegrationEventConsumer(
      NullLogger<GuestDraftPickRevealedIntegrationEventConsumer>.Instance,
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
  public async Task Handle_ShouldSendPickRevealedMethodAsync()
  {
    // Arrange
    var integrationEvent = BuildEvent();
    var hubContext = new TestHubContext();
    var consumer = new GuestDraftPickRevealedIntegrationEventConsumer(
      NullLogger<GuestDraftPickRevealedIntegrationEventConsumer>.Instance,
      hubContext
    );

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Method.Should().Be("PickRevealed");
  }

  [Fact]
  public async Task Handle_ShouldPassCorrectArgsInOrderAsync()
  {
    // Arrange
    var guestDraftPublicId = "gd_pub";
    var playOrder = 4;
    var boardPosition = 6;
    var moviePublicId = "m_abc123";
    var playedByParticipantId = Guid.NewGuid();

    var integrationEvent = new GuestDraftPickRevealedIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: guestDraftPublicId,
      pickId: Guid.NewGuid(),
      playOrder: playOrder,
      boardPosition: boardPosition,
      moviePublicId: moviePublicId,
      playedByParticipantId: playedByParticipantId
    );

    var hubContext = new TestHubContext();
    var consumer = new GuestDraftPickRevealedIntegrationEventConsumer(
      NullLogger<GuestDraftPickRevealedIntegrationEventConsumer>.Instance,
      hubContext
    );

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    var args = hubContext.SentMessages.Single().Args;
    args.Should().HaveCount(5);
    args[0].Should().Be(guestDraftPublicId);
    args[1].Should().Be(playOrder);
    args[2].Should().Be(boardPosition);
    args[3].Should().Be(moviePublicId);
    args[4].Should().Be(playedByParticipantId);
  }

  private static GuestDraftPickRevealedIntegrationEvent BuildEvent(string guestDraftPublicId = "gd_test") =>
    new(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: guestDraftPublicId,
      pickId: Guid.NewGuid(),
      playOrder: 1,
      boardPosition: 1,
      moviePublicId: "m_test",
      playedByParticipantId: Guid.NewGuid()
    );
}
