using FastEndpoints;

namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.ExportAuthAuditLogs;

internal sealed class Endpoint(
  IDbConnectionFactory connectionFactory,
  ExportHelpers<AuthAuditLogCsvRow> exportHelpers)
  : Endpoint<ExportAuthAuditLogsRequest>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly ExportHelpers<AuthAuditLogCsvRow> _exportHelpers = exportHelpers;

  public override void Configure()
  {
    Get(AuditRoutes.AuthLogsExport);
    Description(x =>
    {
      x.WithName(AuditOpenApi.Names.Audit_ExportAuthAuditLogs)
       .WithTags(AuditOpenApi.Tag)
       .Produces<FileResult>(StatusCodes.Status200OK, "text/csv");
    });
    Policies(AuditAuth.Permission.AuditExport);
  }

  public override async Task HandleAsync(ExportAuthAuditLogsRequest req, CancellationToken ct)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(ct);

    var sql = new StringBuilder(
    """
            SELECT 
              id AS Id,
              occurred_on_utc AS OccurredOnUtc,
              event_type AS EventType,
              user_id AS UserId,
              client_id AS ClientId,
              ip_address AS IpAddress
            FROM audit.auth_audit_logs
            WHERE 1=1
            """);

    var parameters = new DynamicParameters();

    if (!string.IsNullOrWhiteSpace(req.UserId))
    {
      sql.Append(" AND user_id = @user_id");
      parameters.Add("user_id", req.UserId);
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

    sql.Append(" ORDER BY occurred_on_utc DESC LIMIT 100000");

    var rows = await connection.QueryAsync<AuthAuditLogCsvRow>(
      new CommandDefinition(
        sql.ToString(),
        parameters,
        cancellationToken: ct));

    await _exportHelpers.WriteCsvAsync(rows, "auth_audit_logs.csv", ct);
  }
}
