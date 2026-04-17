namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.ExportHttpAuditLogs;

internal sealed class HttpAuditLogCsvRow
{
  public Guid Id { get; init; }
  public Guid CorrelationId { get; init; }
  public DateTimeOffset OccurredOnUtc { get; init; }
  public string? ActorId { get; init; }
  public string EndpointName { get; init; } = string.Empty;
  public string HttpMethod { get; init; } = string.Empty;
  public string Route { get; init; } = string.Empty;
  public int? StatusCode { get; init; }
  public int? DurationMs { get; init; }
  public string? IpAddress { get; init; }
}
