using FastEndpoints;

namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.ExportHttpAuditLogs;

internal sealed class Endpoint(
  IDbConnectionFactory connectionFactory,
  ExportHelpers<HttpAuditLogCsvRow> exportHelpers)
  : Endpoint<ExportHttpAuditLogsRequest>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly ExportHelpers<HttpAuditLogCsvRow> _exportHelpers = exportHelpers;

  public override void Configure()
  {
    Get(AuditRoutes.HttpLogsExport);
    Description(x =>
    {
      x.WithName(AuditOpenApi.Names.Audit_ExportHttpAuditLogs)
       .WithTags(AuditOpenApi.Tag)
       .Produces<FileResult>(StatusCodes.Status200OK, "text/csv");
    });
    Policies(AuditAuth.Permission.AuditExport);
  }

  public override async Task HandleAsync(ExportHttpAuditLogsRequest req, CancellationToken ct)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(ct);

    var sql = new StringBuilder(
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

    if (req.StatusCode.HasValue)
    {
      sql.Append(" AND status_code = @status_code");
      parameters.Add("status_code", req.StatusCode.Value);
    }

    if (!string.IsNullOrWhiteSpace(req.Endpoint))
    {
      sql.Append(" AND endpoint_name ILIKE @endpoint");
      parameters.Add("endpoint", $"%{req.Endpoint}%");
    }

    sql.Append(" ORDER BY occurred_on_utc DESC LIMIT 100000");

    var rows = await connection.QueryAsync<HttpAuditLogCsvRow>(
      new CommandDefinition(
        sql.ToString(),
        parameters,
        cancellationToken: ct));

    await _exportHelpers.WriteCsvAsync(rows, "http_audit_logs.csv", ct);
  }
}
