namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.GetDomainEventAuditLogs;

internal sealed record GetDomainEventAuditLogsRequest
{
  [FromQuery(Name = "actorId")]
  public string? ActorId { get; init; }

  [FromQuery(Name = "from")]
  public DateTimeOffset? From { get; init; }

  [FromQuery(Name = "to")]
  public DateTimeOffset? To { get; init; }

  [FromQuery(Name = "eventType")]
  public string? EventType { get; init; }

  [FromQuery(Name = "sourceModule")]
  public string? SourceModule { get; init; }

  [FromQuery(Name = "cursorId")]
  public Guid? CursorId { get; init; }

  [FromQuery(Name = "pageSize")]
  public int PageSize { get; init; } = 50;
}

