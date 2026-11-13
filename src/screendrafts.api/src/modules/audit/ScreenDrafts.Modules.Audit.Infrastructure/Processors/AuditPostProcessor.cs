namespace ScreenDrafts.Modules.Audit.Infrastructure.Processors;

internal sealed class AuditPostProcessor(
  IDateTimeProvider dateTimeProvider,
  IServiceScopeFactory serviceScopeFactory)
  : IGlobalPostProcessor
{
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
  private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

  public async Task PostProcessAsync(IPostProcessorContext context, CancellationToken ct)
  {
    var httpContext = context.HttpContext;

    var correlationId = AuditPreProcessor.GetCorrelationId(httpContext) ?? Guid.NewGuid();

    var stopwatch = AuditPreProcessor.GetStopwatch(httpContext);
    stopwatch?.Stop();

    var actorId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
      ?? httpContext.User.FindFirstValue("sub");

    var requestBody = AuditPreProcessor.GetRequestBody(httpContext);

    string? responseBody = null;

    if (context.Response is { } resp)
    {
      try
      {
        responseBody = System.Text.Json.JsonSerializer.Serialize(resp);
      }
      catch (Exception ex)
      {
        throw new ScreenDraftsException("Failed to serialize response body.", ex);
      }
    }

    var endpointDefinition = context.HttpContext.GetEndpoint();
    var endpointName = endpointDefinition?.Metadata
      .GetMetadata<EndpointNameMetadata>()?.EndpointName
      ?? httpContext.Request.Path.ToString();

    var log = new HttpAuditLog(
      correlationId,
      correlationId,
      _dateTimeProvider.UtcTimeZoneNow,
      actorId,
      endpointName,
      httpContext.Request.Method,
      httpContext.Request.Path.ToString(),
      httpContext.Response.StatusCode,
      (int)(stopwatch?.ElapsedMilliseconds ?? 0),
      requestBody,
      responseBody,
      httpContext.Connection.RemoteIpAddress?.ToString());

    await using var scope = _serviceScopeFactory.CreateAsyncScope();
    var writeService = scope.ServiceProvider.GetRequiredService<IAuditWriteService>();

    // `ct` is the request's token: it is cancelled when the client disconnects (a navigation,
    // a refresh, an aborted fetch), which dropped the audit row and threw here. The row must
    // be written either way, so give the write a short timeout of its own.
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
    await writeService.WriteHttpLogAsync(log, timeout.Token);
  }
}
