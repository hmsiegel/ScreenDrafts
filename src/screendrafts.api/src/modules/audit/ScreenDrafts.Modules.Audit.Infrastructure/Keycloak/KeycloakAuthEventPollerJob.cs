namespace ScreenDrafts.Modules.Audit.Infrastructure.Keycloak;

internal sealed partial class KeycloakAuthEventPollerJob(
  IDbConnectionFactory connectionFactory,
  IAuditWriteService auditWriteService,
  IOptions<KeycloakPollerOptions> pollerOptions,
  ILogger<KeycloakAuthEventPollerJob> logger) : IJob
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly IAuditWriteService _auditWriteService = auditWriteService;
  private readonly KeycloakPollerOptions _pollerOptions = pollerOptions.Value;
  private readonly ILogger<KeycloakAuthEventPollerJob> _logger = logger;

  private const string WatermarkQuery =
    """
        SELECT last_event_time
        FROM audit.auth_event_watermark
        LIMIT 1;
        """;

  private const string UpsertWatermark =
      """
        INSERT INTO audit.auth_event_watermark (id, last_event_time)
        VALUES (1, @last_event_time)
        ON CONFLICT (id) DO UPDATE
            SET last_event_time = EXCLUDED.last_event_time;
        """;

  [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Warning,
    Message = "[Audit] Keycloak poller could not obtain admin token.")]
  private static partial void LogUnableToObtainToken(ILogger<KeycloakAuthEventPollerJob> logger);

  [LoggerMessage(
    EventId = 2,
    Level = LogLevel.Information,
    Message = "[Audit] Keycloak poller fetched {Count} new auth events")]
  private static partial void LogJobFetchedEvents(ILogger<KeycloakAuthEventPollerJob> logger, int count);

  [LoggerMessage(
    EventId = 3,
    Level = LogLevel.Error,
    Message = "[Audit] Keycloak poller failed to fetch auth events: {ErrorMessage}")]
  private static partial void LogJobFetchError(ILogger<KeycloakAuthEventPollerJob> logger, string errorMessage);

  [LoggerMessage(
    EventId = 4,
    Level = LogLevel.Warning,
    Message = "[Audit] Keycloak events API returned {StatusCode}")]
  private static partial void LogEventsApiError(ILogger<KeycloakAuthEventPollerJob> logger, string statusCode);

  [LoggerMessage(
    EventId = 5,
    Level = LogLevel.Error,
    Message = "[Audit] Keycloak poller job failed with exception: {ExceptionMessage}")]
  private static partial void LogJobException(ILogger<KeycloakAuthEventPollerJob> logger, string exceptionMessage);

  [LoggerMessage(
    EventId = 6,
    Level = LogLevel.Information,
    Message = "[Audit] Keycloak poller Execute called")]
  private static partial void LogKeycloakPollerExecuteCalled(ILogger<KeycloakAuthEventPollerJob> logger);

  public async Task Execute(IJobExecutionContext context)
  {
    LogKeycloakPollerExecuteCalled(_logger);

    try
    {
      var token = await GetAdminTokenAsync(context.CancellationToken);

      if (string.IsNullOrEmpty(token))
      {
        LogUnableToObtainToken(_logger);
        return;
      }

      var watermark = await GetWatermarkAsync(context.CancellationToken);

      var events = await FetchEventsAsync(token, watermark, context.CancellationToken);

      if (events.Count == 0)
      {
        return;
      }

      LogJobFetchedEvents(_logger, events.Count);

      DateTimeOffset? latestEventTime = null;

      foreach (var evt in events)
      {
        var log = new AuthAuditLog(
          Guid.NewGuid(),
          evt.OccurredOnUtc,
          evt.Type ?? "unknown",
          evt.UserId,
          evt.ClientId,
          evt.IpAddress,
          evt.Details is not null
            ? System.Text.Json.JsonSerializer.Serialize(evt.Details, Common.Infrastructure.Serialization.SerializerOptions.Instance)
            : null);

        await _auditWriteService.WriteAuthLogAsync(log, context.CancellationToken);

        if (latestEventTime is null || evt.OccurredOnUtc > latestEventTime)
        {
          latestEventTime = evt.OccurredOnUtc;
        }
      }

      if (latestEventTime.HasValue)
      {
        await UpdateWatermarkAsync(latestEventTime.Value, context.CancellationToken);
      }
    }
    catch (HttpRequestException ex)
    {
      LogJobException(_logger, ex.Message);
    }
  }

  private async Task<string?> GetAdminTokenAsync(CancellationToken ct)
  {
    using var client = new HttpClient();

    using var body = new FormUrlEncodedContent(
    [
      new KeyValuePair<string, string>("grant_type", "client_credentials"),
      new KeyValuePair<string, string>("client_id", _pollerOptions.ConfidentialClientId),
      new KeyValuePair<string, string>("client_secret", _pollerOptions.ConfidentialClientSecret)
    ]);

    var response = await client.PostAsync(new Uri(_pollerOptions.TokenUrl), body, ct);

    if (!response.IsSuccessStatusCode)
    {
      return null;
    }

    var json = await response.Content.ReadAsStringAsync(ct);
    using var doc = JsonDocument.Parse(json);

    return doc.RootElement
      .TryGetProperty("access_token", out var tokenElement)
      ? tokenElement.GetString()
      : null;
  }

  private async Task<List<KeycloakEventDto>> FetchEventsAsync(
    string token,
    DateTimeOffset? since,
    CancellationToken ct)
  {
    using var client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    var dateFrom = since.HasValue
      ? since.Value.AddDays(-1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
      : DateTimeOffset.UtcNow.AddDays(-1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    var url = $"{_pollerOptions.AdminUrl}events?dateFrom={dateFrom}&max=500";

    var response = await client.GetAsync(new Uri(url), ct);

    if (!response.IsSuccessStatusCode)
    {
      LogEventsApiError(_logger, response.StatusCode.ToString());
      return [];
    }

    var json = await response.Content.ReadAsStringAsync(ct);
    var allEvents = System.Text.Json.JsonSerializer.Deserialize<List<KeycloakEventDto>>(json,
      Common.Infrastructure.Serialization.SerializerOptions.Instance)
      ?? [];

    return since.HasValue
      ? allEvents.Where(e => e.OccurredOnUtc > since.Value).ToList()
      : allEvents;
  }

  private async Task<DateTimeOffset?> GetWatermarkAsync(CancellationToken ct)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(ct);
    var result = await connection.QuerySingleOrDefaultAsync<DateTime?>(WatermarkQuery);
    return result.HasValue ? new DateTimeOffset(result.Value, TimeSpan.Zero) : null;
  }

  private async Task UpdateWatermarkAsync(DateTimeOffset timestamp, CancellationToken ct)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(ct);
    await connection.ExecuteAsync(UpsertWatermark, new { last_event_time = timestamp });
  }

  private sealed class KeycloakEventDto
  {
    public string? Type { get; init; } = default!;
    public string? UserId { get; init; } = default!;
    public string? ClientId { get; init; } = default!;
    public string? IpAddress { get; init; } = default!;
    public Dictionary<string, string>? Details { get; init; } = default!;
    // Keycloak returns "time" as Unix milliseconds
    public long Time { get; init; } = default!;

    public DateTimeOffset OccurredOnUtc =>
        DateTimeOffset.FromUnixTimeMilliseconds(Time);
  }
}
