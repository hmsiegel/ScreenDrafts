namespace ScreenDrafts.Modules.RealTimeUpdates.IntegrationTests.DraftParts;

public sealed class PickAddedConsumerTests
{
  // -------------------------------------------------------------------------
  // Group routing
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldNotifyCorrectHubGroupAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var integrationEvent = new PickAddedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, draftPartId, "tt1234567", "Test Movie");
    var hubContext = new TestHubContext();
    var consumer = new PickAddedIntegrationEventConsumer(
      hubContext,
      NullLogger<PickAddedIntegrationEventConsumer>.Instance);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Should().ContainSingle()
      .Which.GroupName.Should().Be(DraftHub.GroupName(draftPartId.ToString()));
  }

  [Fact]
  public async Task Handle_ShouldSendPickListUpdatedMethodAsync()
  {
    // Arrange
    var integrationEvent = new PickAddedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), "tt1234567", "Test Movie");
    var hubContext = new TestHubContext();
    var consumer = new PickAddedIntegrationEventConsumer(
      hubContext,
      NullLogger<PickAddedIntegrationEventConsumer>.Instance);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Method.Should().Be("PickListUpdated");
  }

  [Fact]
  public async Task Handle_ShouldPassDraftPartIdAsArgumentAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var imdbId = "tt1234567";
    var movieTitle = "Test Movie";
    var integrationEvent = new PickAddedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, draftPartId, imdbId, movieTitle);
    var hubContext = new TestHubContext();
    var consumer = new PickAddedIntegrationEventConsumer(
      hubContext,
      NullLogger<PickAddedIntegrationEventConsumer>.Instance);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    var args = hubContext.SentMessages.Single().Args;
    args.Should().HaveCount(3);
    args[0].Should().Be(draftPartId);
    args[1].Should().Be(imdbId);
    args[2].Should().Be(movieTitle);
  }
}

public sealed class VetoAppliedConsumerTests
{
  [Fact]
  public async Task Handle_ShouldNotifyCorrectHubGroupAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var integrationEvent = new VetoAppliedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, draftPartId);
    var hubContext = new TestHubContext();
    var consumer = new VetoAppliedIntegrationEventConsumer(
      hubContext,
      NullLogger<VetoAppliedIntegrationEventConsumer>.Instance);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Should().ContainSingle()
      .Which.GroupName.Should().Be(DraftHub.GroupName(draftPartId.ToString()));
  }

  [Fact]
  public async Task Handle_ShouldSendPickListUpdatedMethodAsync()
  {
    // Arrange
    var integrationEvent = new VetoAppliedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid());
    var hubContext = new TestHubContext();
    var consumer = new VetoAppliedIntegrationEventConsumer(
      hubContext,
      NullLogger<VetoAppliedIntegrationEventConsumer>.Instance);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Method.Should().Be("PickListUpdated");
  }

  [Fact]
  public async Task Handle_ShouldPassDraftPartIdAsArgumentAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var integrationEvent = new VetoAppliedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, draftPartId);
    var hubContext = new TestHubContext();
    var consumer = new VetoAppliedIntegrationEventConsumer(
      hubContext,
      NullLogger<VetoAppliedIntegrationEventConsumer>.Instance);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Args
      .Should().ContainSingle()
      .Which.Should().Be(draftPartId);
  }
}

public sealed class VetoOverrideAppliedConsumerTests
{
  [Fact]
  public async Task Handle_ShouldNotifyCorrectHubGroupAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var integrationEvent = new VetoOverrideAppliedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, draftPartId);
    var hubContext = new TestHubContext();
    var consumer = new VetoOverrideAppliedIntegrationEventConsumer(
      hubContext,
      NullLogger<VetoOverrideAppliedIntegrationEventConsumer>.Instance);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Should().ContainSingle()
      .Which.GroupName.Should().Be(DraftHub.GroupName(draftPartId.ToString()));
  }

  [Fact]
  public async Task Handle_ShouldSendPickListUpdatedMethodAsync()
  {
    // Arrange
    var integrationEvent = new VetoOverrideAppliedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid());
    var hubContext = new TestHubContext();
    var consumer = new VetoOverrideAppliedIntegrationEventConsumer(
      hubContext,
      NullLogger<VetoOverrideAppliedIntegrationEventConsumer>.Instance);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Method.Should().Be("PickListUpdated");
  }

  [Fact]
  public async Task Handle_ShouldPassDraftPartIdAsArgumentAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var integrationEvent = new VetoOverrideAppliedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, draftPartId);
    var hubContext = new TestHubContext();
    var consumer = new VetoOverrideAppliedIntegrationEventConsumer(
      hubContext,
      NullLogger<VetoOverrideAppliedIntegrationEventConsumer>.Instance);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Args
      .Should().ContainSingle()
      .Which.Should().Be(draftPartId);
  }
}

public sealed class DraftPositionAssignedConsumerTests
{
  [Fact]
  public async Task Handle_ShouldNotifyCorrectHubGroupAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var integrationEvent = new DraftPositionAssignedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, draftPartId, Guid.NewGuid(), Guid.NewGuid(), 1);
    var hubContext = new TestHubContext();
    var consumer = new DraftPositionAssignedIntegrationEventConsumer(
      NullLogger<DraftPositionAssignedIntegrationEventConsumer>.Instance,
      hubContext);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Should().ContainSingle()
      .Which.GroupName.Should().Be(DraftHub.GroupName(draftPartId.ToString()));
  }

  [Fact]
  public async Task Handle_ShouldSendDraftPositionAssignedMethodAsync()
  {
    // Arrange
    var integrationEvent = new DraftPositionAssignedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1);
    var hubContext = new TestHubContext();
    var consumer = new DraftPositionAssignedIntegrationEventConsumer(
      NullLogger<DraftPositionAssignedIntegrationEventConsumer>.Instance,
      hubContext);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Method.Should().Be("DraftPositionAssigned");
  }

  [Fact]
  public async Task Handle_ShouldPassCorrectArgumentsAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var draftPositionId = Guid.NewGuid();
    var participantId = Guid.NewGuid();
    const int participantKind = 1;
    var integrationEvent = new DraftPositionAssignedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, draftPartId, draftPositionId, participantId, participantKind);
    var hubContext = new TestHubContext();
    var consumer = new DraftPositionAssignedIntegrationEventConsumer(
      NullLogger<DraftPositionAssignedIntegrationEventConsumer>.Instance,
      hubContext);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    var args = hubContext.SentMessages.Single().Args;
    args.Should().HaveCount(4);
    args[0].Should().Be(draftPartId);
    args[1].Should().Be(draftPositionId);
    args[2].Should().Be(participantId);
    args[3].Should().Be(participantKind);
  }
}

public sealed class DraftPositionUnassignedConsumerTests
{
  [Fact]
  public async Task Handle_ShouldNotifyCorrectHubGroupAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var integrationEvent = new DraftPositionUnassignedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, draftPartId, Guid.NewGuid());
    var hubContext = new TestHubContext();
    var consumer = new DraftPositionUnassignedIntegrationEventConsumer(
      NullLogger<DraftPositionUnassignedIntegrationEventConsumer>.Instance,
      hubContext);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Should().ContainSingle()
      .Which.GroupName.Should().Be(DraftHub.GroupName(draftPartId.ToString()));
  }

  [Fact]
  public async Task Handle_ShouldSendDraftPositionUnassignedMethodAsync()
  {
    // Arrange
    var integrationEvent = new DraftPositionUnassignedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid());
    var hubContext = new TestHubContext();
    var consumer = new DraftPositionUnassignedIntegrationEventConsumer(
      NullLogger<DraftPositionUnassignedIntegrationEventConsumer>.Instance,
      hubContext);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Method.Should().Be("DraftPositionUnassigned");
  }

  [Fact]
  public async Task Handle_ShouldPassCorrectArgumentsAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var draftPositionId = Guid.NewGuid();
    var integrationEvent = new DraftPositionUnassignedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, draftPartId, draftPositionId);
    var hubContext = new TestHubContext();
    var consumer = new DraftPositionUnassignedIntegrationEventConsumer(
      NullLogger<DraftPositionUnassignedIntegrationEventConsumer>.Instance,
      hubContext);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    var args = hubContext.SentMessages.Single().Args;
    args.Should().HaveCount(2);
    args[0].Should().Be(draftPartId);
    args[1].Should().Be(draftPositionId);
  }
}

public sealed class CommissionerOverrideAppliedConsumerTests
{
  [Fact]
  public async Task Handle_ShouldNotifyCorrectHubGroupAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var integrationEvent = new CommissionerOverrideAppliedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, draftPartId);
    var hubContext = new TestHubContext();
    var consumer = new CommissionerOverrideAppliedIntegrationEventConsumer(
      hubContext,
      NullLogger<CommissionerOverrideAppliedIntegrationEventConsumer>.Instance);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Should().ContainSingle()
      .Which.GroupName.Should().Be(DraftHub.GroupName(draftPartId.ToString()));
  }

  [Fact]
  public async Task Handle_ShouldSendPickListUpdatedMethodAsync()
  {
    // Arrange
    var integrationEvent = new CommissionerOverrideAppliedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid());
    var hubContext = new TestHubContext();
    var consumer = new CommissionerOverrideAppliedIntegrationEventConsumer(
      hubContext,
      NullLogger<CommissionerOverrideAppliedIntegrationEventConsumer>.Instance);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Method.Should().Be("PickListUpdated");
  }

  [Fact]
  public async Task Handle_ShouldPassDraftPartIdAsArgumentAsync()
  {
    // Arrange
    var draftPartId = Guid.NewGuid();
    var integrationEvent = new CommissionerOverrideAppliedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, draftPartId);
    var hubContext = new TestHubContext();
    var consumer = new CommissionerOverrideAppliedIntegrationEventConsumer(
      hubContext,
      NullLogger<CommissionerOverrideAppliedIntegrationEventConsumer>.Instance);

    // Act
    await consumer.Handle(integrationEvent, CancellationToken.None);

    // Assert
    hubContext.SentMessages.Single().Args
      .Should().ContainSingle()
      .Which.Should().Be(draftPartId);
  }
}
