namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.GetAuthAuditLogs;

internal sealed record AuthAuditLogResponse
{
  public Guid Id { get; init; }
  public DateTimeOffset OccurredOnUtc { get; init; }
  public string EventType { get; init; } = default!;
  public string? UserId { get; init; }
  public string? ClientId { get; init; }
  public string? IpAddress { get; init; }
  public string? Details { get; init; }
}

