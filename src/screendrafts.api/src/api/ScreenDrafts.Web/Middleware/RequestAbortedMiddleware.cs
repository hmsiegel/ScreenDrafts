namespace ScreenDrafts.Web.Middleware;

internal sealed partial class RequestAbortedExceptionMiddleware(
  RequestDelegate next,
  ILogger<RequestAbortedExceptionMiddleware> logger
)
{
  private readonly RequestDelegate _next = next;
  private readonly ILogger<RequestAbortedExceptionMiddleware> _logger = logger;

  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await _next(context);
    }
    catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
    {
      LogRequestAborted(_logger, context.Request.Path);
    }
  }

  [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Debug,
    Message = "Request aborted by client: {Path}"
  )]
  private static partial void LogRequestAborted(
    ILogger<RequestAbortedExceptionMiddleware> logger,
    PathString path
  );
}

internal static class RequestAbortedExceptionMiddlewareExtensions
{
  public static IApplicationBuilder UseRequestAbortedHandling(this IApplicationBuilder app) =>
    app.UseMiddleware<RequestAbortedExceptionMiddleware>();
}
