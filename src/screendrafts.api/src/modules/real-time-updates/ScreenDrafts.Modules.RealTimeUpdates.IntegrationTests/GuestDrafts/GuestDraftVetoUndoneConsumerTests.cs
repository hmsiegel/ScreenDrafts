namespace ScreenDrafts.Modules.RealTimeUpdates.IntegrationTests.GuestDrafts;

public sealed class GuestDraftVetoUndoneConsumerTests
{
  [Fact]
  public async Task Handle_ShouldNotifyTheFlatGuestDraftGroupAsync()
  {
    // Arrange
    var guestDraftPublicId = "gd_test123";
    var integrationEvent = BuildEvent(guestDraftPublicId: guestDraftPublicId);
    var hubContext = new TestHubContext();
    var consumer = new GuestDraftVetoUndoneIntegrationEventConsumer(
      NullLogger<GuestDraftVetoUndoneIntegrationEventConsumer>.Instance,
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
  public async Task Handle_ShouldSendVetoUndoneMethodAsync()
  {
    // Arrange
    var integrationEvent = BuildEvent();
    var hubContext = new TestHubContext();
    var consumer = new GuestDraftVetoUndoneIntegrationEventConsumer(
      NullLogger<GuestDraftVetoUndoneIntegrationEventConsumer>.Instance,
      hubContext
    );

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Method.Should().Be("VetoUndone");
  }

  [Fact]
  public async Task Handle_ShouldPassCorrectArgsInOrderAsync()
  {
    // Arrange
    var guestDraftPublicId = "gd_pub";
    var playOrder = 2;
    var moviePublicId = "m_abc123";
    int? vetoTokensRemaining = 1;
    int? overrideTokensRemaining = 0;

    var integrationEvent = new GuestDraftVetoUndoneIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: guestDraftPublicId,
      pickId: Guid.NewGuid(),
      playOrder: playOrder,
      moviePublicId: moviePublicId,
      refundedToParticipantId: Guid.NewGuid(),
      vetoTokensRemaining: vetoTokensRemaining,
      overrideTokensRemaining: overrideTokensRemaining
    );

    var hubContext = new TestHubContext();
    var consumer = new GuestDraftVetoUndoneIntegrationEventConsumer(
      NullLogger<GuestDraftVetoUndoneIntegrationEventConsumer>.Instance,
      hubContext
    );

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    var args = hubContext.SentMessages.Single().Args;
    args.Should().HaveCount(5);
    args[0].Should().Be(guestDraftPublicId);
    args[1].Should().Be(playOrder);
    args[2].Should().Be(moviePublicId);
    args[3].Should().Be(vetoTokensRemaining);
    args[4].Should().Be(overrideTokensRemaining);
  }

  private static GuestDraftVetoUndoneIntegrationEvent BuildEvent(string guestDraftPublicId = "gd_test") =>
    new(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: guestDraftPublicId,
      pickId: Guid.NewGuid(),
      playOrder: 1,
      moviePublicId: "m_test",
      refundedToParticipantId: Guid.NewGuid(),
      vetoTokensRemaining: 1,
      overrideTokensRemaining: 0
    );
}
