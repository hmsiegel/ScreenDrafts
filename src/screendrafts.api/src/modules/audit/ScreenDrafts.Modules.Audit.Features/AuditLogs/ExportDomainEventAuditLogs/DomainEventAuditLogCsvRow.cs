namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.ExportDomainEventAuditLogs;

internal sealed class DomainEventAuditLogCsvRow
{
  public Guid Id { get; init; }
  public DateTimeOffset OccurredOnUtc { get; init; }
  public string EventType { get; init; } = string.Empty;
  public string SourceModule { get; init; } = string.Empty;
  public string? ActorId { get; init; }
  public string? EntityId { get; init; }
}
