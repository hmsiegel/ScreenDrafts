namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.ExportDomainEventAuditLogs;

internal sealed record ExportDomainEventAuditLogsRequest
{
  [FromQuery(Name = "actorId")]
  public string? ActorId { get; init; }

  [FromQuery(Name = "from")]
  public DateTimeOffset? From { get; init; }

  [FromQuery(Name = "to")]
  public DateTimeOffset? To { get; init; }

  [FromQuery(Name = "eventType")]
  public string? EventType { get; init; }

  [FromQuery(Name = "sourceModule")]
  public string? SourceModule { get; init; }
}
