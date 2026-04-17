namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.GetDomainEventAuditLogs;

internal sealed class GetDomainEventAuditLogQueryHandler(IDbConnectionFactory connectionFactory)
  : IQueryHandler<GetDomainEventAuditLogsQuery, GetDomainEventAuditLogsResponse>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public async Task<Result<GetDomainEventAuditLogsResponse>> Handle(GetDomainEventAuditLogsQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      """
      SELECT 
        id AS Id,
        occurred_on_utc AS OccurredOnUtc,
        event_type AS EventType,
        source_module AS SourceModule,
        actor_id AS ActorId,
        entity_id AS EntityId,
        payload AS Payload
      FROM audit.domain_event_audit_logs
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

    if (!string.IsNullOrWhiteSpace(request.EventType))
    {
      sb.Append(" AND event_type ILIKE @event_type");
      parameters.Add("event_type", $"%{request.EventType}%");
    }

    if (!string.IsNullOrWhiteSpace(request.SourceModule))
    {
      sb.Append(" AND source_module = @source_module");
      parameters.Add("source_module", request.SourceModule);
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

    var rows = await connection.QueryAsync<DomainEventAuditLogRow>(
      new CommandDefinition(
        sb.ToString(),
        parameters,
        cancellationToken: cancellationToken));

    var list = rows.ToList();

    var hasMore = list.Count > request.PageSize;

    var items = hasMore ? list.Take(request.PageSize).ToList() : list;

    var mapped = items.Select(r => new DomainEventAuditLogResponse
    {
      Id = r.Id,
      OccurredOnUtc = new DateTimeOffset(r.OccurredOnUtc, TimeSpan.Zero),
      EventType = r.EventType,
      SourceModule = r.SourceModule,
      ActorId = r.ActorId,
      EntityId = r.EntityId,
      Payload = r.Payload
    }).ToList();

    var nextCursor = hasMore ? items[^1].Id : (Guid?)null;

    return new GetDomainEventAuditLogsResponse
    {
      Items = mapped,
       NextCursor = nextCursor,
       HasMoreItems = hasMore,
    };
    
  }

  private sealed record DomainEventAuditLogRow(
    Guid Id,
    DateTime OccurredOnUtc,
    string EventType,
    string SourceModule,
    string? ActorId,
    string? EntityId,
    string Payload);
}

