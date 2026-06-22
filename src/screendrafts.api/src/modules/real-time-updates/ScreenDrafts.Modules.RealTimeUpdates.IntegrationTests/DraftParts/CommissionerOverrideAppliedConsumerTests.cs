using Bogus;

namespace ScreenDrafts.Modules.RealTimeUpdates.IntegrationTests.DraftParts;

public sealed class CommissionerOverrideAppliedConsumerTests
{
  [Fact]
  public async Task Handle_ShouldNotifyCorrectHubGroupAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var draftPartPublicId = "dp_test123";
    var integrationEvent = new CommissionerOverrideAppliedIntegrationEvent(
      Guid.NewGuid(),
      DateTime.UtcNow,
      draftPartId,
      draftPartPublicId,
      12345,
      "Test Movie",
      Guid.NewGuid(),
      0,
      1
    );
    var hubContext = new TestHubContext();
    var fakeDb = new FakeDbConnectionFactory();
    var consumer = new CommissionerOverrideAppliedIntegrationEventConsumer(
      hubContext,
      NullLogger<CommissionerOverrideAppliedIntegrationEventConsumer>.Instance,
      fakeDb
    );

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext
      .SentMessages.Should()
      .ContainSingle()
      .Which.GroupName.Should()
      .Be(DraftHub.GroupName(draftPartPublicId));
  }

  [Fact]
  public async Task Handle_ShouldSendPickListUpdatedMethodAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var draftPartPublicId = "dp_test123";
    var integrationEvent = new CommissionerOverrideAppliedIntegrationEvent(
      Guid.NewGuid(),
      DateTime.UtcNow,
      draftPartId,
      draftPartPublicId,
      12345,
      "Test Movie",
      Guid.NewGuid(),
      0,
      1
    );
    var hubContext = new TestHubContext();
    var fakeDb = new FakeDbConnectionFactory();
    var consumer = new CommissionerOverrideAppliedIntegrationEventConsumer(
      hubContext,
      NullLogger<CommissionerOverrideAppliedIntegrationEventConsumer>.Instance,
      fakeDb
    );

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Method.Should().Be("CommissionerOverrideApplied");
  }

  [Fact]
  public async Task Handle_ShouldPassDraftPartIdAsArgumentAsync()
  {
    // Arrange
    var participantId = Guid.NewGuid();
    var draftPartId = Guid.NewGuid();
    var draftPartPublicId = "dp_test123";
    var integrationEvent = new CommissionerOverrideAppliedIntegrationEvent(
      Guid.NewGuid(),
      DateTime.UtcNow,
      draftPartId,
      draftPartPublicId,
      12345,
      "Test Movie",
      participantId,
      0,
      1
    );
    var hubContext = new TestHubContext();
    var fakeDb = new FakeDbConnectionFactory();
    var consumer = new CommissionerOverrideAppliedIntegrationEventConsumer(
      hubContext,
      NullLogger<CommissionerOverrideAppliedIntegrationEventConsumer>.Instance,
      fakeDb
    );

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Args.Should().ContainSingle()
      .Which.Should().BeEquivalentTo(new
      {
        DraftPartPublicId = draftPartPublicId,
        TmdbId = 12345,
        MovieTitle = "Test Movie",
        ParticipantId = participantId,
        ParticipantKind = 0,
        BoardPosition = 1,
        Participants = Array.Empty<object>(),
      });
  }
}
