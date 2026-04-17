namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.GetHttpAuditLogs;

internal sealed class GetHttpAuditLogQueryHandler(IDbConnectionFactory connectionFactory)
  : IQueryHandler<GetHttpAuditLogQuery, GetHttpAuditLogsResponse>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public async Task<Result<GetHttpAuditLogsResponse>> Handle(GetHttpAuditLogQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      """
      SELECT 
        id AS Id,
        correlation_id AS CorrelationId,
        occurred_on_utc AS OccurredOnUtc,
        actor_id AS ActorId,
        endpoint_name AS EndpointName,
        http_method AS HttpMethod,
        route AS Route,
        status_code AS StatusCode,
        duration_ms AS DurationMs,
        ip_address AS IpAddress
      FROM audit.http_audit_logs
      WHERE 1=1
      """;

    var sb = new StringBuilder(sql);

    var parameters = new DynamicParameters();

    if (!string.IsNullOrWhiteSpace(request.ActorId))
    {
      sb.Append(" AND actor_id = @actor_id");
      parameters.Add("actor_id", request.ActorId);
    }

    if (request.From.HasValue)
    {
      sb.Append(" AND occurred_on_utc >= @from");
      parameters.Add("from", request.From.Value);
    }

    if (request.To.HasValue)
    {
      sb.Append(" AND occurred_on_utc <= @to");
      parameters.Add("to", request.To.Value);
    }

    if (request.StatusCode.HasValue)
    {
      sb.Append(" AND status_code = @status_code");
      parameters.Add("status_code", request.StatusCode.Value);
    }

    if (!string.IsNullOrWhiteSpace(request.Endpoint))
    {
      sb.Append(" AND endpoint_name ILIKE @endpoint");
      parameters.Add("endpoint", $"%{request.Endpoint}%");
    }

    if (request.CursorId.HasValue && request.CursorTimestamp.HasValue)
    {
      sb.Append(" AND (occurred_on_utc, id) < (@cursor_timestamp, @cursor_id)");
      parameters.Add("cursor_timestamp", request.CursorTimestamp.Value);
      parameters.Add("cursor_id", request.CursorId.Value);
    }

    var fetchSize = request.PageSize + 1;
    sb.Append(" ORDER BY occurred_on_utc DESC, id DESC");
    sb.Append(" LIMIT @page_size");
    parameters.Add("page_size", fetchSize);

    var rows = (await connection.QueryAsync<HttpAuditLogRow>(
      new CommandDefinition(
        sb.ToString(),
        parameters,
        cancellationToken: cancellationToken))).ToList();

    var hasMore = rows.Count > request.PageSize;

    var items = hasMore ? [.. rows.Take(request.PageSize)] : rows;

    var mapped = items.Select(r => new HttpAuditLogResponse
    {
      Id = r.Id,
      CorrelationId = r.CorrelationId,
      OccurredOnUtc = new DateTimeOffset(r.OccurredOnUtc, TimeSpan.Zero),
      ActorId = r.ActorId,
      EndpointName = r.EndpointName,
      HttpMethod = r.HttpMethod,
      Route = r.Route,
      StatusCode = r.StatusCode,
      DurationMs = r.DurationMs,
      IpAddress = r.IpAddress
    }).ToList();

    var nextCursor = hasMore ? items[^1].Id : (Guid?)null;
    var nextCursorTimestamp = hasMore ? new DateTimeOffset(items[^1].OccurredOnUtc, TimeSpan.Zero) : (DateTimeOffset?)null;

    return new GetHttpAuditLogsResponse
    {
      Items = mapped,
      NextCursor = nextCursor,
      NextCursorTimestamp = nextCursorTimestamp,
      HasMoreItems = hasMore,
    };
  }

  private sealed record HttpAuditLogRow(
    Guid Id,
    Guid CorrelationId,
    DateTime OccurredOnUtc,
    string? ActorId,
    string EndpointName,
    string HttpMethod,
    string Route,
    int? StatusCode,
    int? DurationMs,
    string? IpAddress);
}

