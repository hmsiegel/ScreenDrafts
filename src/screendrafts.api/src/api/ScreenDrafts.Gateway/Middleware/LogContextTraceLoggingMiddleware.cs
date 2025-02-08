namespace ScreenDrafts.Gateway.Middleware;
internal sealed class LogContextTraceLoggingMiddleware(RequestDelegate next)
{
  private readonly RequestDelegate _next = next;

  public Task Invoke(HttpContext context)
  {
    var traceId = Activity.Current?.TraceId.ToString() ?? "null";

    using (LogContext.PushProperty("TraceId", traceId))
    {
      return _next.Invoke(context);
    }
  }
}
