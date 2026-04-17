namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.GetHttpAuditLogs;

internal sealed record GetHttpAuditLogQuery : IQuery<GetHttpAuditLogsResponse>
{
  public string? ActorId { get; init; }
  public DateTimeOffset? From { get; init; }
  public DateTimeOffset? To { get; init; }
  public int? StatusCode { get; init; }
  public string? Endpoint { get; init; }
  public Guid? CursorId { get; init; }
  public DateTimeOffset? CursorTimestamp { get; init; }
  public int PageSize { get; init; }
}

