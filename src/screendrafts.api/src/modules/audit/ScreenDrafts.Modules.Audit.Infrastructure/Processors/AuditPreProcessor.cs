using System.Diagnostics;

using FastEndpoints;

using ScreenDrafts.Modules.Audit.Infrastructure.Redaction;

namespace ScreenDrafts.Modules.Audit.Infrastructure.Processors;

internal sealed class AuditPreProcessor : IGlobalPreProcessor
{
  private const string CorrelationIdKey  = "Audit:CorrelationId";
  private const string StopwatchKey = "Audit:Stopwatch";
  private const string RequestBodyKey = "Audit:RequestBody";

  public async Task PreProcessAsync(IPreProcessorContext context, CancellationToken ct)
  {
    var httpContext = context.HttpContext;

    var correlationId = Guid.NewGuid();
    var stopwatch = Stopwatch.StartNew();

    httpContext.Items[CorrelationIdKey] = correlationId;
    httpContext.Items[StopwatchKey] = stopwatch;

    string? requestBody = null;

    if (!BodyRedactionPolicy.ShouldRedact(httpContext.Request.Path))
    {
      httpContext.Request.EnableBuffering();

      using var reader = new StreamReader(httpContext.Request.Body, leaveOpen: true);

      var raw = await reader.ReadToEndAsync(ct);

      httpContext.Request.Body.Position = 0;

      if (!string.IsNullOrWhiteSpace(raw))
      {
        requestBody = raw;
      }
    }

    httpContext.Items[RequestBodyKey] = requestBody;

    await Task.CompletedTask;
  }

  internal static Guid? GetCorrelationId(HttpContext httpContext) =>
    httpContext.Items.TryGetValue(CorrelationIdKey, out var v) ? v as Guid? : null;

  internal static Stopwatch? GetStopwatch(HttpContext httpContext) =>
    httpContext.Items.TryGetValue(StopwatchKey, out var v) ? (Stopwatch?)v : null;

  internal static string? GetRequestBody(HttpContext httpContext) =>
     httpContext.Items.TryGetValue(RequestBodyKey, out var v) ? v as string : null;
}
