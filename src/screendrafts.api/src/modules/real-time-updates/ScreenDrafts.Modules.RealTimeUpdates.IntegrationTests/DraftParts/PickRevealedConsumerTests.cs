namespace ScreenDrafts.Modules.RealTimeUpdates.IntegrationTests.DraftParts;

public sealed class PickRevealedConsumerTests
{
  // -------------------------------------------------------------------------
  // Group routing
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldNotifyHostGroupAsync()
  {
    // Arrange
    var draftPartPublicId = "dp_abc123xyz";
    var integrationEvent = BuildEvent(draftPartPublicId: draftPartPublicId);
    var hubContext = new TestHubContext();
    var consumer = new PickRevealedIntegrationEventConsumer(
      NullLogger<PickRevealedIntegrationEventConsumer>.Instance,
      hubContext);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Should().ContainSingle()
      .Which.GroupName.Should().Be(DraftHub.HostGroupName(draftPartPublicId));
  }

  // -------------------------------------------------------------------------
  // Method name
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldSendPickRevealedMethodAsync()
  {
    // Arrange
    var integrationEvent = BuildEvent();
    var hubContext = new TestHubContext();
    var consumer = new PickRevealedIntegrationEventConsumer(
      NullLogger<PickRevealedIntegrationEventConsumer>.Instance,
      hubContext);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Method.Should().Be("PickRevealed");
  }

  // -------------------------------------------------------------------------
  // Payload args
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldPassCorrectArgsAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var playOrder = 3;
    var moviePublicId = "m_abc123";
    var movieTitle = "Test Movie";
    int? tmdbId = 12345;
    var boardPosition = 2;
    var participantId = Guid.NewGuid();
    var participantKind = 0;

    var integrationEvent = new PickRevealedIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      draftPartId: draftPartId,
      draftPartPublicId: "dp_pub",
      playOrder: playOrder,
      movieId: Guid.NewGuid(),
      moviePublicId: moviePublicId,
      movieTitle: movieTitle,
      imdbId: "tt1234567",
      tmdbId: tmdbId,
      boardPosition: boardPosition,
      participantId: participantId,
      participantKind: participantKind,
      actedByPublicId: null);

    var hubContext = new TestHubContext();
    var consumer = new PickRevealedIntegrationEventConsumer(
      NullLogger<PickRevealedIntegrationEventConsumer>.Instance,
      hubContext);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    var args = hubContext.SentMessages.Single().Args;
    args.Should().HaveCount(8);
    args[0].Should().Be(draftPartId);
    args[1].Should().Be(playOrder);
    args[2].Should().Be(moviePublicId);
    args[3].Should().Be(movieTitle);
    args[4].Should().Be(tmdbId);
    args[5].Should().Be(boardPosition);
    args[6].Should().Be(participantId);
    args[7].Should().Be(participantKind);
  }

  // -------------------------------------------------------------------------
  // Helper
  // -------------------------------------------------------------------------

  private static PickRevealedIntegrationEvent BuildEvent(string draftPartPublicId = "dp_test") =>
    new(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      draftPartId: Guid.NewGuid(),
      draftPartPublicId: draftPartPublicId,
      playOrder: 1,
      movieId: Guid.NewGuid(),
      moviePublicId: "m_test",
      movieTitle: "Test Movie",
      imdbId: "tt0000001",
      tmdbId: 99,
      boardPosition: 1,
      participantId: Guid.NewGuid(),
      participantKind: 0,
      actedByPublicId: null);
}
