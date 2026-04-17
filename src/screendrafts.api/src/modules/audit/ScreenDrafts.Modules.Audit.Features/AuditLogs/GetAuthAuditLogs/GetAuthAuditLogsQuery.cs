namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.GetAuthAuditLogs;

internal sealed record GetAuthAuditLogsQuery : IQuery<GetAuthAuditLogsResponse>
{
  public string? UserId { get; init; }
  public string? EventType { get; init; }
  public DateTimeOffset? From { get; init; }
  public DateTimeOffset? To { get; init; }
  public Guid? CursorId { get; init; }
  public int PageSize { get; init; }
}

