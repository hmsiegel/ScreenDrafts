namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.GetAuthAuditLogs;

internal sealed class GetAuthAuditLogQueryHandler(IDbConnectionFactory connectionFactory)
  : IQueryHandler<GetAuthAuditLogsQuery, GetAuthAuditLogsResponse>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public async Task<Result<GetAuthAuditLogsResponse>> Handle(GetAuthAuditLogsQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      """
      SELECT 
        id AS Id, 
        occurred_on_utc AS OccurredOnUtc,
        event_type AS EventType,
        user_id AS UserId, 
        client_id AS ClientId,
        ip_address AS IpAddress,
        details::text AS Details
      FROM audit.auth_audit_logs
      WHERE 1=1
      """;

    var sb = new StringBuilder(sql);

    var parameters = new DynamicParameters();

    if (!string.IsNullOrWhiteSpace(request.UserId))
    {
      sb.Append(" AND user_id = @user_id");
      parameters.Add("user_id", request.UserId);
    }

    if (!string.IsNullOrWhiteSpace(request.EventType))
    {
      sb.Append(" AND event_type ILIKE @event_type");
      parameters.Add("event_type", $"%{request.EventType}%");
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

    if (request.CursorId.HasValue)
    {
      sb.Append(" AND id > @cursor_id");
      parameters.Add("cursor_id", request.CursorId.Value);
    }

    var fetchSize = request.PageSize + 1;
    sb.Append(" ORDER BY occurred_on_utc DESC, id DESC");
    sb.Append(" LIMIT @page_size");
    parameters.Add("page_size", fetchSize);

    var rows = await connection.QueryAsync<AuthAuditLogRow>(
      new CommandDefinition(
        sb.ToString(),
        parameters,
        cancellationToken: cancellationToken));

    var list = rows.ToList();

    var hasMore = list.Count > request.PageSize;

    var items = hasMore ? [.. list.Take(request.PageSize)] : list;

    var mapped = items.Select(r => new AuthAuditLogResponse
    {
      Id = r.Id,
      OccurredOnUtc = new DateTimeOffset(r.OccurredOnUtc, TimeSpan.Zero),
      EventType = r.EventType,
      UserId = r.UserId,
      ClientId = r.ClientId,
      IpAddress = r.IpAddress,
      Details = r.Details
    }).ToList();

    var nextCursor = hasMore ? items[^1].Id : (Guid?)null;

    return new GetAuthAuditLogsResponse
    {
      Items = mapped,
      NextCursor = nextCursor,
      HasMoreItems = hasMore,
    };

  }

  private sealed record AuthAuditLogRow(
    Guid Id,
    DateTime OccurredOnUtc,
    string EventType,
    string? UserId,
    string? ClientId,
    string? IpAddress,
    string? Details);
}
