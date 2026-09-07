namespace ScreenDrafts.Modules.RealTimeUpdates.IntegrationTests.GuestDrafts;

public sealed class GuestDraftVetoAppliedConsumerTests
{
  [Fact]
  public async Task Handle_ShouldNotifyTheFlatGuestDraftGroupAsync()
  {
    // Arrange
    var guestDraftPublicId = "gd_test123";
    var integrationEvent = BuildEvent(guestDraftPublicId: guestDraftPublicId);
    var hubContext = new TestHubContext();
    var consumer = new GuestDraftVetoAppliedIntegrationEventConsumer(
      NullLogger<GuestDraftVetoAppliedIntegrationEventConsumer>.Instance,
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
  public async Task Handle_ShouldSendVetoAppliedMethodAsync()
  {
    // Arrange
    var integrationEvent = BuildEvent();
    var hubContext = new TestHubContext();
    var consumer = new GuestDraftVetoAppliedIntegrationEventConsumer(
      NullLogger<GuestDraftVetoAppliedIntegrationEventConsumer>.Instance,
      hubContext
    );

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Method.Should().Be("VetoApplied");
  }

  [Fact]
  public async Task Handle_ShouldPassCorrectArgsInOrderAsync()
  {
    // Arrange
    var guestDraftPublicId = "gd_pub";
    var playOrder = 2;
    var moviePublicId = "m_abc123";
    var vetoedByParticipantId = Guid.NewGuid();
    var playedByParticipantId = Guid.NewGuid();
    var vetoTokensRemaining = 3;
    var overrideTokensRemaining = 1;

    var integrationEvent = new GuestDraftVetoAppliedIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: guestDraftPublicId,
      pickId: Guid.NewGuid(),
      playOrder: playOrder,
      moviePublicId: moviePublicId,
      vetoedByParticipantId: vetoedByParticipantId,
      playedByParticipantId: playedByParticipantId,
      vetoTokensRemaining: vetoTokensRemaining,
      overrideTokensRemaining: overrideTokensRemaining
    );

    var hubContext = new TestHubContext();
    var consumer = new GuestDraftVetoAppliedIntegrationEventConsumer(
      NullLogger<GuestDraftVetoAppliedIntegrationEventConsumer>.Instance,
      hubContext
    );

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    var args = hubContext.SentMessages.Single().Args;
    args.Should().HaveCount(7);
    args[0].Should().Be(guestDraftPublicId);
    args[1].Should().Be(playOrder);
    args[2].Should().Be(moviePublicId);
    args[3].Should().Be(vetoedByParticipantId);
    args[4].Should().Be(playedByParticipantId);
    args[5].Should().Be(vetoTokensRemaining);
    args[6].Should().Be(overrideTokensRemaining);
  }

  private static GuestDraftVetoAppliedIntegrationEvent BuildEvent(string guestDraftPublicId = "gd_test") =>
    new(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: guestDraftPublicId,
      pickId: Guid.NewGuid(),
      playOrder: 1,
      moviePublicId: "m_test",
      vetoedByParticipantId: Guid.NewGuid(),
      playedByParticipantId: Guid.NewGuid(),
      vetoTokensRemaining: 1,
      overrideTokensRemaining: 0
    );
}
