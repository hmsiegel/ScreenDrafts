using ScreenDrafts.Modules.Audit.Features.AuditLogs.GetAuthAuditLogs;

namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.GetDomainEventAuditLogs;

internal sealed record GetDomainEventAuditLogsResponse
{
  public IReadOnlyList<DomainEventAuditLogResponse> Items { get; init; } = [];
  public Guid? NextCursor { get; init; }
  public bool HasMoreItems { get; init; }
}

