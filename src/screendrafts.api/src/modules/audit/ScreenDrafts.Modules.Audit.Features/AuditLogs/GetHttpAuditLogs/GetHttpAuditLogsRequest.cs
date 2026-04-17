namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.GetHttpAuditLogs;

internal sealed record GetHttpAuditLogsRequest
{
  [FromQuery(Name = "actorId")]
  public string? ActorId { get; init; }

  [FromQuery(Name = "from")]
  public DateTimeOffset? From { get; init; }

  [FromQuery(Name = "to")]
  public DateTimeOffset? To { get; init; }
  
  [FromQuery(Name = "statusCode")]
  public int? StatusCode { get; init; }

  [FromQuery(Name = "endpoint")]
  public string? Endpoint { get; init; }

  [FromQuery(Name = "cursorId")]
  public Guid? CursorId { get; init; }

  [FromQuery(Name = "cursorTimestamp")]
  public DateTimeOffset? CursorTimestamp { get; init; }

  [FromQuery(Name = "pageSize")]
  public int PageSize { get; init; } = 50;
}

