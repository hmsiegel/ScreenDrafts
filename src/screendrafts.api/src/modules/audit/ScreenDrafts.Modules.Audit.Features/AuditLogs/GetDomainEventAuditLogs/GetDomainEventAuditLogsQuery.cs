using ScreenDrafts.Modules.Audit.Features.AuditLogs.GetAuthAuditLogs;

namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.GetDomainEventAuditLogs;

internal sealed record GetDomainEventAuditLogsQuery : IQuery<GetDomainEventAuditLogsResponse>
{
  public string? ActorId { get; init; }
  public DateTimeOffset? From { get; init; }
  public DateTimeOffset? To { get; init; }
  public string? EventType { get; init; }
  public string? SourceModule { get; init; }
  public Guid? CursorId { get; init; }
  public int PageSize { get; init; }
}

