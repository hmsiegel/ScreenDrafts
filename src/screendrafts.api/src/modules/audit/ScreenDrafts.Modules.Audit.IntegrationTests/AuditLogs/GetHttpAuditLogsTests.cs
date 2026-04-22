namespace ScreenDrafts.Modules.Audit.IntegrationTests.AuditLogs;

public sealed class GetHttpAuditLogsTests(AuditIntegrationTestWebAppFactory factory)
  : AuditIntegrationTest(factory)
{
  [Fact]
  public async Task Handle_WithNoFilters_ReturnsAllLogsAsync()
  {
    // Arrange
    var log1 = BuildHttpLog(actorId: "actor-1", route: "/api/drafts", statusCode: 200);
    var log2 = BuildHttpLog(actorId: "actor-2", route: "/api/campaigns", statusCode: 404);
    await WriteHttpLogAsync(log1);
    await WriteHttpLogAsync(log2);

    var query = new GetHttpAuditLogQuery { PageSize = 25 };

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
    var log1 = BuildHttpLog(actorId: targetActorId);
    var log2 = BuildHttpLog(actorId: "actor-other");
    await WriteHttpLogAsync(log1);
    await WriteHttpLogAsync(log2);

    var query = new GetHttpAuditLogQuery { ActorId = targetActorId, PageSize = 25 };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(1);
    result.Value.Items[0].ActorId.Should().Be(targetActorId);
  }

  [Fact]
  public async Task Handle_FilterByStatusCode_ReturnsMatchingLogsAsync()
  {
    // Arrange
    var log200 = BuildHttpLog(statusCode: 200);
    var log404 = BuildHttpLog(statusCode: 404);
    var log500 = BuildHttpLog(statusCode: 500);
    await WriteHttpLogAsync(log200);
    await WriteHttpLogAsync(log404);
    await WriteHttpLogAsync(log500);

    var query = new GetHttpAuditLogQuery { StatusCode = 404, PageSize = 25 };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(1);
    result.Value.Items[0].StatusCode.Should().Be(404);
  }

  [Fact]
  public async Task Handle_FilterByEndpoint_ReturnsCaseInsensitiveMatchAsync()
  {
    // Arrange
    var log1 = BuildHttpLog(endpointName: "ScreenDrafts.Modules.Drafts.Features.CreateDraft");
    var log2 = BuildHttpLog(endpointName: "ScreenDrafts.Modules.Users.Features.GetUser");
    await WriteHttpLogAsync(log1);
    await WriteHttpLogAsync(log2);

    var query = new GetHttpAuditLogQuery { Endpoint = "createdraft", PageSize = 25 };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(1);
    result.Value.Items[0].EndpointName.Should().Be("ScreenDrafts.Modules.Drafts.Features.CreateDraft");
  }

  [Fact]
  public async Task Handle_FilterByDateRange_ReturnsLogsWithinRangeAsync()
  {
    // Arrange
    var baseTime = DateTimeOffset.UtcNow.AddDays(-10);
    var oldLog = BuildHttpLog(occurredOnUtc: baseTime.AddDays(-5));
    var inRangeLog = BuildHttpLog(occurredOnUtc: baseTime.AddDays(1));
    var futureLog = BuildHttpLog(occurredOnUtc: baseTime.AddDays(3));
    await WriteHttpLogAsync(oldLog);
    await WriteHttpLogAsync(inRangeLog);
    await WriteHttpLogAsync(futureLog);

    var query = new GetHttpAuditLogQuery
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
    result.Value.Items[0].Id.Should().Be(inRangeLog.Id);
  }

  [Fact]
  public async Task Handle_Pagination_ReturnsCursorAndHasMoreAsync()
  {
    // Arrange — insert 5 logs with staggered timestamps so ordering is stable
    var now = DateTimeOffset.UtcNow;
    for (var i = 0; i < 5; i++)
    {
      await WriteHttpLogAsync(BuildHttpLog(occurredOnUtc: now.AddMinutes(-i)));
    }

    var query = new GetHttpAuditLogQuery { PageSize = 3 };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(3);
    result.Value.HasMoreItems.Should().BeTrue();
    result.Value.NextCursor.Should().NotBeNull();
    result.Value.NextCursorTimestamp.Should().NotBeNull();
  }

  [Fact]
  public async Task Handle_Pagination_SecondPageReturnsRemainingItemsAsync()
  {
    // Arrange — insert 5 logs with staggered timestamps
    var now = DateTimeOffset.UtcNow;
    for (var i = 0; i < 5; i++)
    {
      await WriteHttpLogAsync(BuildHttpLog(occurredOnUtc: now.AddMinutes(-i)));
    }

    var firstPage = new GetHttpAuditLogQuery { PageSize = 3 };
    var firstResult = await Sender.Send(firstPage, TestContext.Current.CancellationToken);
    firstResult.IsSuccess.Should().BeTrue();

    var secondPage = new GetHttpAuditLogQuery
    {
      PageSize = 3,
      CursorId = firstResult.Value.NextCursor,
      CursorTimestamp = firstResult.Value.NextCursorTimestamp,
    };

    // Act
    var result = await Sender.Send(secondPage, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(2);
    result.Value.HasMoreItems.Should().BeFalse();
    result.Value.NextCursor.Should().BeNull();
  }

  [Fact]
  public async Task Handle_EmptyDatabase_ReturnsEmptyListAsync()
  {
    // Arrange
    var query = new GetHttpAuditLogQuery { PageSize = 25 };

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
    var expectedCorrelationId = Guid.NewGuid();
    var expectedOccurredOn = DateTimeOffset.UtcNow.AddHours(-1);
    var log = new HttpAuditLog(
      Id: expectedId,
      CorrelationId: expectedCorrelationId,
      OccurredOnUtc: expectedOccurredOn,
      ActorId: "actor-abc",
      EndpointName: "TestEndpoint",
      HttpMethod: "GET",
      Route: "/api/test",
      StatusCode: 200,
      DurationMs: 42,
      RequestBody: null,
      ResponseBody: null,
      IpAddress: "127.0.0.1");

    await WriteHttpLogAsync(log);

    var query = new GetHttpAuditLogQuery { PageSize = 25 };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var item = result.Value.Items.Should().ContainSingle().Subject;
    item.Id.Should().Be(expectedId);
    item.CorrelationId.Should().Be(expectedCorrelationId);
    item.ActorId.Should().Be("actor-abc");
    item.EndpointName.Should().Be("TestEndpoint");
    item.HttpMethod.Should().Be("GET");
    item.Route.Should().Be("/api/test");
    item.StatusCode.Should().Be(200);
    item.DurationMs.Should().Be(42);
    item.IpAddress.Should().Be("127.0.0.1");
  }

  private static HttpAuditLog BuildHttpLog(
    string? actorId = null,
    string? endpointName = null,
    string? route = null,
    int? statusCode = null,
    DateTimeOffset? occurredOnUtc = null) =>
    new(
      Id: Guid.NewGuid(),
      CorrelationId: Guid.NewGuid(),
      OccurredOnUtc: occurredOnUtc ?? DateTimeOffset.UtcNow,
      ActorId: actorId,
      EndpointName: endpointName ?? "TestEndpoint",
      HttpMethod: "GET",
      Route: route ?? "/api/test",
      StatusCode: statusCode,
      DurationMs: 10,
      RequestBody: null,
      ResponseBody: null,
      IpAddress: null);
}
