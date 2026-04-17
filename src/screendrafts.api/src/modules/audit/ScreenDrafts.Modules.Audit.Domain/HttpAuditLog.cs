namespace ScreenDrafts.Modules.Audit.Domain;

public sealed record HttpAuditLog(
    Guid Id,
    Guid CorrelationId,
    DateTimeOffset OccurredOnUtc,
    string? ActorId,
    string EndpointName,
    string HttpMethod,
    string Route,
    int? StatusCode,
    int? DurationMs,
    string? RequestBody,
    string? ResponseBody,
    string? IpAddress);
