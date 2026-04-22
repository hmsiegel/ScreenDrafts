namespace ScreenDrafts.Modules.Audit.IntegrationTests.AuditLogs;

public sealed class GetAuthAuditLogsTests(AuditIntegrationTestWebAppFactory factory)
  : AuditIntegrationTest(factory)
{
  [Fact]
  public async Task Handle_WithNoFilters_ReturnsAllLogsAsync()
  {
    // Arrange
    await WriteAuthLogAsync(BuildAuthLog(eventType: "LOGIN"));
    await WriteAuthLogAsync(BuildAuthLog(eventType: "LOGOUT"));

    var query = new GetAuthAuditLogsQuery { PageSize = 25 };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(2);
  }

  [Fact]
  public async Task Handle_FilterByUserId_ReturnsMatchingLogsAsync()
  {
    // Arrange
    var targetUserId = "user-target";
    await WriteAuthLogAsync(BuildAuthLog(userId: targetUserId));
    await WriteAuthLogAsync(BuildAuthLog(userId: "user-other"));

    var query = new GetAuthAuditLogsQuery { UserId = targetUserId, PageSize = 25 };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(1);
    result.Value.Items[0].UserId.Should().Be(targetUserId);
  }

  [Fact]
  public async Task Handle_FilterByEventType_ReturnsCaseInsensitiveMatchAsync()
  {
    // Arrange
    await WriteAuthLogAsync(BuildAuthLog(eventType: "LOGIN"));
    await WriteAuthLogAsync(BuildAuthLog(eventType: "LOGOUT"));
    await WriteAuthLogAsync(BuildAuthLog(eventType: "LOGIN_ERROR"));

    var query = new GetAuthAuditLogsQuery { EventType = "login_error", PageSize = 25 };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(1);
    result.Value.Items[0].EventType.Should().Be("LOGIN_ERROR");
  }

  [Fact]
  public async Task Handle_FilterByEventType_PartialMatchReturnsMultipleAsync()
  {
    // Arrange
    await WriteAuthLogAsync(BuildAuthLog(eventType: "LOGIN"));
    await WriteAuthLogAsync(BuildAuthLog(eventType: "LOGIN_ERROR"));
    await WriteAuthLogAsync(BuildAuthLog(eventType: "LOGOUT"));

    var query = new GetAuthAuditLogsQuery { EventType = "login", PageSize = 25 };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(2);
  }

  [Fact]
  public async Task Handle_FilterByDateRange_ReturnsLogsWithinRangeAsync()
  {
    // Arrange
    var baseTime = DateTimeOffset.UtcNow.AddDays(-10);
    await WriteAuthLogAsync(BuildAuthLog(occurredOnUtc: baseTime.AddDays(-5)));
    await WriteAuthLogAsync(BuildAuthLog(occurredOnUtc: baseTime.AddDays(1)));
    await WriteAuthLogAsync(BuildAuthLog(occurredOnUtc: baseTime.AddDays(3)));

    var query = new GetAuthAuditLogsQuery
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
      await WriteAuthLogAsync(BuildAuthLog(occurredOnUtc: now.AddMinutes(-i)));
    }

    var query = new GetAuthAuditLogsQuery { PageSize = 3 };

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
    var query = new GetAuthAuditLogsQuery { PageSize = 25 };

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
    var log = new AuthAuditLog(
      Id: expectedId,
      OccurredOnUtc: DateTimeOffset.UtcNow.AddHours(-1),
      EventType: "LOGIN",
      UserId: "user-abc",
      ClientId: "screendrafts-public-client",
      IpAddress: "10.0.0.1",
      Details: """{"session":"abc123"}""");

    await WriteAuthLogAsync(log);

    var query = new GetAuthAuditLogsQuery { PageSize = 25 };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var item = result.Value.Items.Should().ContainSingle().Subject;
    item.Id.Should().Be(expectedId);
    item.EventType.Should().Be("LOGIN");
    item.UserId.Should().Be("user-abc");
    item.ClientId.Should().Be("screendrafts-public-client");
    item.IpAddress.Should().Be("10.0.0.1");
    item.Details.Should().Contain("abc123");
  }

  private static AuthAuditLog BuildAuthLog(
    string? userId = null,
    string? eventType = null,
    DateTimeOffset? occurredOnUtc = null) =>
    new(
      Id: Guid.NewGuid(),
      OccurredOnUtc: occurredOnUtc ?? DateTimeOffset.UtcNow,
      EventType: eventType ?? "LOGIN",
      UserId: userId,
      ClientId: "screendrafts-public-client",
      IpAddress: null,
      Details: null);
}
