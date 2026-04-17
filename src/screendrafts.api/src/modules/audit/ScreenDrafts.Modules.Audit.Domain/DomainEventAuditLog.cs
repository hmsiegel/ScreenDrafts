namespace ScreenDrafts.Modules.Audit.Domain;

public sealed record DomainEventAuditLog(
  Guid Id,
  DateTimeOffset OccurredOnUtc,
  string EventType,
  string SourceModule,
  string? ActorId,
  string? EntityId,
  string Payload);

