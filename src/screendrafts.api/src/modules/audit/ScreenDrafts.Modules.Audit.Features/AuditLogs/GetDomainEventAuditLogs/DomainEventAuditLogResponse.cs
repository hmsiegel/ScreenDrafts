namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.GetDomainEventAuditLogs;

internal sealed record DomainEventAuditLogResponse
{
  public Guid Id { get; init; }
  public DateTimeOffset OccurredOnUtc { get; init; }
  public string EventType { get; init; } = default!;
  public string SourceModule { get; init; } = default!;
  public string? ActorId { get; init; }
  public string? EntityId { get; init; }
  public string Payload { get; init; } = default!;
}

