namespace ScreenDrafts.Modules.RealTimeUpdates.IntegrationTests.DraftParts;

public sealed class PickSubmittedConsumerTests
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
    var consumer = new PickSubmittedIntegrationEventConsumer(
      NullLogger<PickSubmittedIntegrationEventConsumer>.Instance,
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
  public async Task Handle_ShouldSendPickSubmittedMethodAsync()
  {
    // Arrange
    var integrationEvent = BuildEvent();
    var hubContext = new TestHubContext();
    var consumer = new PickSubmittedIntegrationEventConsumer(
      NullLogger<PickSubmittedIntegrationEventConsumer>.Instance,
      hubContext);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Method.Should().Be("PickSubmitted");
  }

  // -------------------------------------------------------------------------
  // Payload args
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldPassCorrectArgsAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var playOrder = 2;
    var moviePublicId = "m_xyz456";
    var movieTitle = "Another Movie";
    int? tmdbId = 54321;
    var participantId = Guid.NewGuid();
    var participantKind = 1;

    var integrationEvent = new PickSubmittedIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      draftPartId: draftPartId,
      draftPartPublicId: "dp_pub",
      playOrder: playOrder,
      movieId: Guid.NewGuid(),
      moviePublicId: moviePublicId,
      movieTitle: movieTitle,
      imdbId: "tt7654321",
      tmdbId: tmdbId,
      boardPosition: 1,
      participantId: participantId,
      participantKind: participantKind,
      actedByPublicId: null);

    var hubContext = new TestHubContext();
    var consumer = new PickSubmittedIntegrationEventConsumer(
      NullLogger<PickSubmittedIntegrationEventConsumer>.Instance,
      hubContext);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    var args = hubContext.SentMessages.Single().Args;
    args.Should().HaveCount(7);
    args[0].Should().Be(draftPartId);
    args[1].Should().Be(playOrder);
    args[2].Should().Be(moviePublicId);
    args[3].Should().Be(movieTitle);
    args[4].Should().Be(tmdbId);
    args[5].Should().Be(participantId);
    args[6].Should().Be(participantKind);
  }

  // -------------------------------------------------------------------------
  // Helper
  // -------------------------------------------------------------------------

  private static PickSubmittedIntegrationEvent BuildEvent(string draftPartPublicId = "dp_test") =>
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
