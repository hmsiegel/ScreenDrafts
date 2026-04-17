namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.GetAuthAuditLogs;

internal sealed record GetAuthAuditLogsResponse
{
  public IReadOnlyList<AuthAuditLogResponse> Items { get; init; } = [];
  public Guid? NextCursor { get; init; }
  public bool HasMoreItems { get; init; }
}

