namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.ExportAuthAuditLogs;

internal sealed class AuthAuditLogCsvRow
{
  public Guid Id { get; init; }
  public DateTimeOffset OccurredOnUtc { get; init; }
  public string EventType { get; init; } = string.Empty;
  public string? UserId { get; init; }
  public string? ClientId { get; init; }
  public string? IpAddress { get; init; }
}
