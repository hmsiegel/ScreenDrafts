namespace ScreenDrafts.Modules.Audit.Domain;

public sealed record AuthAuditLog(Guid Id,
  DateTimeOffset OccurredOnUtc,
  string EventType,
  string? UserId,
  string? ClientId,
  string? IpAddress,
  string? Details);
