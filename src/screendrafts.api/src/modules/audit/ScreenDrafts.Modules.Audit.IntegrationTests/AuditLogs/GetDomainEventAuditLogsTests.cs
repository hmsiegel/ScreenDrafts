namespace ScreenDrafts.Modules.Audit.IntegrationTests.AuditLogs;

public sealed class GetDomainEventAuditLogsTests(AuditIntegrationTestWebAppFactory factory)
  : AuditIntegrationTest(factory)
{
  [Fact]
  public async Task Handle_WithNoFilters_ReturnsAllLogsAsync()
  {
    // Arrange
    await WriteDomainEventLogAsync(BuildDomainEventLog(eventType: "DraftCreated", sourceModule: "Drafts"));
    await WriteDomainEventLogAsync(BuildDomainEventLog(eventType: "CampaignCreated", sourceModule: "Drafts"));

    var query = new GetDomainEventAuditLogsQuery { PageSize = 25 };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(2);
  }

  [Fact]
  public async Task Handle_FilterByActorId_ReturnsMatchingLogsAsync()
  {
    // Arrange
    var targetActorId = "actor-target";
    await WriteDomainEventLogAsync(BuildDomainEventLog(actorId: targetActorId));
    await WriteDomainEventLogAsync(BuildDomainEventLog(actorId: "actor-other"));

    var query = new GetDomainEventAuditLogsQuery { ActorId = targetActorId, PageSize = 25 };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(1);
    result.Value.Items[0].ActorId.Should().Be(targetActorId);
  }

  [Fact]
  public async Task Handle_FilterByEventType_ReturnsCaseInsensitiveMatchAsync()
  {
    // Arrange
    await WriteDomainEventLogAsync(BuildDomainEventLog(eventType: "DraftCreatedDomainEvent"));
    await WriteDomainEventLogAsync(BuildDomainEventLog(eventType: "DraftCompletedDomainEvent"));

    var query = new GetDomainEventAuditLogsQuery { EventType = "draftcreated", PageSize = 25 };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(1);
    result.Value.Items[0].EventType.Should().Be("DraftCreatedDomainEvent");
  }

  [Fact]
  public async Task Handle_FilterBySourceModule_ReturnsMatchingLogsAsync()
  {
    // Arrange
    await WriteDomainEventLogAsync(BuildDomainEventLog(sourceModule: "Drafts"));
    await WriteDomainEventLogAsync(BuildDomainEventLog(sourceModule: "Users"));

    var query = new GetDomainEventAuditLogsQuery { SourceModule = "Users", PageSize = 25 };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(1);
    result.Value.Items[0].SourceModule.Should().Be("Users");
  }

  [Fact]
  public async Task Handle_FilterByDateRange_ReturnsLogsWithinRangeAsync()
  {
    // Arrange
    var baseTime = DateTimeOffset.UtcNow.AddDays(-10);
    await WriteDomainEventLogAsync(BuildDomainEventLog(occurredOnUtc: baseTime.AddDays(-5)));
    await WriteDomainEventLogAsync(BuildDomainEventLog(occurredOnUtc: baseTime.AddDays(1)));
    await WriteDomainEventLogAsync(BuildDomainEventLog(occurredOnUtc: baseTime.AddDays(3)));

    var query = new GetDomainEventAuditLogsQuery
    {
      From = baseTime,
      To = baseTime.AddDays(2),
      PageSize = 25,
    };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(1);
  }

  [Fact]
  public async Task Handle_Pagination_ReturnsCursorAndHasMoreAsync()
  {
    // Arrange
    var now = DateTimeOffset.UtcNow;
    for (var i = 0; i < 5; i++)
    {
      await WriteDomainEventLogAsync(BuildDomainEventLog(occurredOnUtc: now.AddMinutes(-i)));
    }

    var query = new GetDomainEventAuditLogsQuery { PageSize = 3 };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(3);
    result.Value.HasMoreItems.Should().BeTrue();
    result.Value.NextCursor.Should().NotBeNull();
  }

  [Fact]
  public async Task Handle_EmptyDatabase_ReturnsEmptyListAsync()
  {
    // Arrange
    var query = new GetDomainEventAuditLogsQuery { PageSize = 25 };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
    result.Value.HasMoreItems.Should().BeFalse();
  }

  [Fact]
  public async Task Handle_ResponseContainsExpectedFields_AllPropertiesMappedAsync()
  {
    // Arrange
    var expectedId = Guid.NewGuid();
    var expectedPayload = """{"key":"value"}""";
    var log = new DomainEventAuditLog(
      Id: expectedId,
      OccurredOnUtc: DateTimeOffset.UtcNow.AddHours(-1),
      EventType: "DraftCreatedDomainEvent",
      SourceModule: "Drafts",
      ActorId: "actor-xyz",
      EntityId: "entity-123",
      Payload: expectedPayload);

    await WriteDomainEventLogAsync(log);

    var query = new GetDomainEventAuditLogsQuery { PageSize = 25 };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var item = result.Value.Items.Should().ContainSingle().Subject;
    item.Id.Should().Be(expectedId);
    item.EventType.Should().Be("DraftCreatedDomainEvent");
    item.SourceModule.Should().Be("Drafts");
    item.ActorId.Should().Be("actor-xyz");
    item.EntityId.Should().Be("entity-123");
    // PostgreSQL normalises JSONB (adds spaces after colons/commas), so compare key content
    item.Payload.Should().Contain("key").And.Contain("value");
  }

  private static DomainEventAuditLog BuildDomainEventLog(
    string? actorId = null,
    string? eventType = null,
    string? sourceModule = null,
    DateTimeOffset? occurredOnUtc = null) =>
    new(
      Id: Guid.NewGuid(),
      OccurredOnUtc: occurredOnUtc ?? DateTimeOffset.UtcNow,
      EventType: eventType ?? "TestDomainEvent",
      SourceModule: sourceModule ?? "Drafts",
      ActorId: actorId,
      EntityId: null,
      Payload: """{"test":true}""");
}
