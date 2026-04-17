using FastEndpoints;

namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.ExportDomainEventAuditLogs;

internal sealed class Endpoint(
  IDbConnectionFactory connectionFactory,
  ExportHelpers<DomainEventAuditLogCsvRow> exportHelpers)
  : Endpoint<ExportDomainEventAuditLogsRequest>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly ExportHelpers<DomainEventAuditLogCsvRow> _exportHelpers = exportHelpers;

  public override void Configure()
  {
    Get(AuditRoutes.DomainEventLogsExport);
    Description(x =>
    {
      x.WithName(AuditOpenApi.Names.Audit_ExportDomainEventAuditLogs)
       .WithTags(AuditOpenApi.Tag)
       .Produces<FileResult>(StatusCodes.Status200OK, "text/csv");
    });
    Policies(AuditAuth.Permission.AuditExport);
  }

  public override async Task HandleAsync(ExportDomainEventAuditLogsRequest req, CancellationToken ct)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(ct);

    var sql = new StringBuilder(
    """
            SELECT 
              id AS Id,
              occurred_on_utc AS OccurredOnUtc
              event_type AS EventType,
              source_module AS SourceModule,
              actor_id AS ActorId,
              entity_id AS EntityId
            FROM audit.domain_event_audit_logs
            WHERE 1=1
            """);

    var parameters = new DynamicParameters();

    if (!string.IsNullOrWhiteSpace(req.ActorId))
    {
      sql.Append(" AND actor_id = @actor_id");
      parameters.Add("actor_id", req.ActorId);
    }

    if (req.From.HasValue)
    {
      sql.Append(" AND occurred_on_utc >= @from");
      parameters.Add("from", req.From.Value);
    }

    if (req.To.HasValue)
    {
      sql.Append(" AND occurred_on_utc <= @to");
      parameters.Add("to", req.To.Value);
    }

    if (!string.IsNullOrWhiteSpace(req.EventType))
    {
      sql.Append(" AND event_type ILIKE @event_type");
      parameters.Add("event_type", $"%{req.EventType}%");
    }

    if (!string.IsNullOrWhiteSpace(req. SourceModule))
    {
      sql.Append(" AND source_module = @source_module");
      parameters.Add("source_module", req.SourceModule);
    }

    sql.Append(" ORDER BY occurred_on_utc DESC LIMIT 100000");

    var rows = await connection.QueryAsync<DomainEventAuditLogCsvRow>(
      new CommandDefinition(
        sql.ToString(),
        parameters,
        cancellationToken: ct));

    await _exportHelpers.WriteCsvAsync(rows, "domain_event_audit_logs.csv", ct);
  }
}
