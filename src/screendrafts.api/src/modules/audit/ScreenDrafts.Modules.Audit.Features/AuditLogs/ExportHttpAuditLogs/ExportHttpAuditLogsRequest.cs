namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.ExportHttpAuditLogs;

internal sealed record ExportHttpAuditLogsRequest
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
}
