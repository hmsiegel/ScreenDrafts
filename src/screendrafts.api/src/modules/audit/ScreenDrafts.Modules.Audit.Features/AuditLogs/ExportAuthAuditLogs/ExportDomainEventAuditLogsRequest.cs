namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.ExportAuthAuditLogs;

internal sealed record ExportAuthAuditLogsRequest
{
  [FromQuery(Name = "actorId")]
  public string? UserId { get; init; }

  [FromQuery(Name = "eventType")]
  public string? EventType { get; init; }

  [FromQuery(Name = "from")]
  public DateTimeOffset? From { get; init; }

  [FromQuery(Name = "to")]
  public DateTimeOffset? To { get; init; }
}
