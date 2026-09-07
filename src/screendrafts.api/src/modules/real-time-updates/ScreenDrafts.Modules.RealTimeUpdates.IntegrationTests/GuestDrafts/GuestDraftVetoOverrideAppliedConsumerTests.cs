namespace ScreenDrafts.Modules.RealTimeUpdates.IntegrationTests.GuestDrafts;

public sealed class GuestDraftVetoOverrideAppliedConsumerTests
{
  [Fact]
  public async Task Handle_ShouldNotifyTheFlatGuestDraftGroupAsync()
  {
    // Arrange
    var guestDraftPublicId = "gd_test123";
    var integrationEvent = BuildEvent(guestDraftPublicId: guestDraftPublicId);
    var hubContext = new TestHubContext();
    var consumer = new GuestDraftVetoOverrideAppliedIntegrationEventConsumer(
      NullLogger<GuestDraftVetoOverrideAppliedIntegrationEventConsumer>.Instance,
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
  public async Task Handle_ShouldSendVetoOverrideAppliedMethodAsync()
  {
    // Arrange
    var integrationEvent = BuildEvent();
    var hubContext = new TestHubContext();
    var consumer = new GuestDraftVetoOverrideAppliedIntegrationEventConsumer(
      NullLogger<GuestDraftVetoOverrideAppliedIntegrationEventConsumer>.Instance,
      hubContext
    );

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Method.Should().Be("VetoOverrideApplied");
  }

  [Fact]
  public async Task Handle_ShouldPassCorrectArgsInOrderAsync()
  {
    // Arrange
    var guestDraftPublicId = "gd_pub";
    var playOrder = 5;
    var moviePublicId = "m_abc123";
    var overriddenByParticipantId = Guid.NewGuid();
    var vetoTokensRemaining = 2;
    var overrideTokensRemaining = 0;

    var integrationEvent = new GuestDraftVetoOverrideAppliedIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: guestDraftPublicId,
      pickId: Guid.NewGuid(),
      playOrder: playOrder,
      moviePublicId: moviePublicId,
      overriddenByParticipantId: overriddenByParticipantId,
      vetoTokensRemaining: vetoTokensRemaining,
      overrideTokensRemaining: overrideTokensRemaining
    );

    var hubContext = new TestHubContext();
    var consumer = new GuestDraftVetoOverrideAppliedIntegrationEventConsumer(
      NullLogger<GuestDraftVetoOverrideAppliedIntegrationEventConsumer>.Instance,
      hubContext
    );

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    var args = hubContext.SentMessages.Single().Args;
    args.Should().HaveCount(6);
    args[0].Should().Be(guestDraftPublicId);
    args[1].Should().Be(playOrder);
    args[2].Should().Be(moviePublicId);
    args[3].Should().Be(overriddenByParticipantId);
    args[4].Should().Be(vetoTokensRemaining);
    args[5].Should().Be(overrideTokensRemaining);
  }

  private static GuestDraftVetoOverrideAppliedIntegrationEvent BuildEvent(string guestDraftPublicId = "gd_test") =>
    new(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: guestDraftPublicId,
      pickId: Guid.NewGuid(),
      playOrder: 1,
      moviePublicId: "m_test",
      overriddenByParticipantId: Guid.NewGuid(),
      vetoTokensRemaining: 1,
      overrideTokensRemaining: 0
    );
}
