namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.GetHttpAuditLogs;

internal sealed record HttpAuditLogResponse
{
  public Guid Id { get; init; }
  public Guid CorrelationId { get; init; }
  public DateTimeOffset OccurredOnUtc { get; init; }
  public string? ActorId { get; init; }
  public string EndpointName { get; init; } = default!;
  public string HttpMethod { get; init; } = default!;
  public string Route { get; init; } = default!;
  public int? StatusCode { get; init; }
  public int? DurationMs { get; init; }
  public string? IpAddress { get; init; }
}

