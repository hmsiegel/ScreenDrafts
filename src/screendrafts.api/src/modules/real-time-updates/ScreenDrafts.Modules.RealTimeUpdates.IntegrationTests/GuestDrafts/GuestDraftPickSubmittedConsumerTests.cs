namespace ScreenDrafts.Modules.RealTimeUpdates.IntegrationTests.GuestDrafts;

/// <summary>
/// This is the one place in the guest-draft real-time pipeline that enforces
/// pre-reveal secrecy: a submitted pick's movie must reach only the participant
/// authorized to reveal it, and only once that authorization has actually been
/// resolved (RevealAuthorizedParticipantId is null until then).
/// </summary>
public sealed class GuestDraftPickSubmittedConsumerTests
{
  // -------------------------------------------------------------------------
  // Case (a) -- RevealAuthorizedParticipantId is set
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldNotifyOnlyTheAuthorizedRevealersParticipantGroupAsync()
  {
    // Arrange
    var guestDraftPublicId = "gd_test123";
    var revealAuthorizedParticipantId = Guid.NewGuid();
    var integrationEvent = BuildEvent(
      guestDraftPublicId: guestDraftPublicId,
      revealAuthorizedParticipantId: revealAuthorizedParticipantId
    );
    var hubContext = new TestHubContext();
    var consumer = new GuestDraftPickSubmittedIntegrationEventConsumer(
      NullLogger<GuestDraftPickSubmittedIntegrationEventConsumer>.Instance,
      hubContext
    );

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert -- exactly one message, to the participant group, never the flat group
    hubContext
      .SentMessages.Should()
      .ContainSingle()
      .Which.GroupName.Should()
      .Be(
        DraftHub.GuestDraftParticipantGroupName(
          guestDraftPublicId,
          revealAuthorizedParticipantId.ToString()
        )
      );
    hubContext
      .SentMessages.Should()
      .NotContain(m => m.GroupName == DraftHub.GuestDraftGroupName(guestDraftPublicId));
  }

  [Fact]
  public async Task Handle_ShouldSendPickSubmittedMethodAsync()
  {
    // Arrange
    var integrationEvent = BuildEvent(revealAuthorizedParticipantId: Guid.NewGuid());
    var hubContext = new TestHubContext();
    var consumer = new GuestDraftPickSubmittedIntegrationEventConsumer(
      NullLogger<GuestDraftPickSubmittedIntegrationEventConsumer>.Instance,
      hubContext
    );

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Method.Should().Be("PickSubmitted");
  }

  [Fact]
  public async Task Handle_ShouldPassCorrectArgsInOrderAsync()
  {
    // Arrange
    var guestDraftPublicId = "gd_pub";
    var playOrder = 1;
    var boardPosition = 7;
    var moviePublicId = "m_abc123";
    var playedByParticipantId = Guid.NewGuid();

    var integrationEvent = new GuestDraftPickSubmittedIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: guestDraftPublicId,
      pickId: Guid.NewGuid(),
      playOrder: playOrder,
      boardPosition: boardPosition,
      moviePublicId: moviePublicId,
      playedByParticipantId: playedByParticipantId,
      actedByPublicId: null,
      revealAuthorizedParticipantId: Guid.NewGuid()
    );

    var hubContext = new TestHubContext();
    var consumer = new GuestDraftPickSubmittedIntegrationEventConsumer(
      NullLogger<GuestDraftPickSubmittedIntegrationEventConsumer>.Instance,
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

  // -------------------------------------------------------------------------
  // Case (b) -- RevealAuthorizedParticipantId is null
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldReturnWithoutCallingTheHubContext_WhenNoRevealerIsAuthorizedYetAsync()
  {
    // Arrange
    var integrationEvent = BuildEvent(revealAuthorizedParticipantId: null);
    var hubContext = new TestHubContext();
    var consumer = new GuestDraftPickSubmittedIntegrationEventConsumer(
      NullLogger<GuestDraftPickSubmittedIntegrationEventConsumer>.Instance,
      hubContext
    );

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert -- nothing broadcast at all, not even to the flat group
    hubContext.SentMessages.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ShouldLogAWarning_WhenNoRevealerIsAuthorizedYetAsync()
  {
    // Arrange
    var integrationEvent = BuildEvent(revealAuthorizedParticipantId: null);
    var hubContext = new TestHubContext();
    var logger = new RecordingLogger<GuestDraftPickSubmittedIntegrationEventConsumer>();
    var consumer = new GuestDraftPickSubmittedIntegrationEventConsumer(logger, hubContext);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    logger
      .Entries.Should()
      .ContainSingle()
      .Which.Level.Should()
      .Be(LogLevel.Warning);
  }

  // -------------------------------------------------------------------------
  // Helper
  // -------------------------------------------------------------------------

  // revealAuthorizedParticipantId has no default -- every call site must state its
  // intent explicitly. A `null`-defaulted parameter would be indistinguishable from
  // a caller explicitly passing `revealAuthorizedParticipantId: null` to build the
  // case-(b) fixture, and an early version of this helper used `?? Guid.NewGuid()`
  // to fill in the "unspecified" case, which silently coerced that explicit null
  // right back into a non-null value -- making the case-(b) tests exercise case (a)
  // instead, undetected until the test run showed them hitting the hub after all.
  private static GuestDraftPickSubmittedIntegrationEvent BuildEvent(
    Guid? revealAuthorizedParticipantId,
    string guestDraftPublicId = "gd_test"
  ) =>
    new(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: guestDraftPublicId,
      pickId: Guid.NewGuid(),
      playOrder: 1,
      boardPosition: 1,
      moviePublicId: "m_test",
      playedByParticipantId: Guid.NewGuid(),
      actedByPublicId: null,
      revealAuthorizedParticipantId: revealAuthorizedParticipantId
    );
}
