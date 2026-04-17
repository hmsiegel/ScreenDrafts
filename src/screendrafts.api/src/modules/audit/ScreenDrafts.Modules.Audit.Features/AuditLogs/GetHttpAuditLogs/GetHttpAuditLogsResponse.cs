namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.GetHttpAuditLogs;

internal sealed record GetHttpAuditLogsResponse
{
  public IReadOnlyList<HttpAuditLogResponse> Items { get; init; } = [];
  public Guid? NextCursor { get; init; }
  public DateTimeOffset? NextCursorTimestamp { get; init; }
  public bool HasMoreItems { get; init; }
}

