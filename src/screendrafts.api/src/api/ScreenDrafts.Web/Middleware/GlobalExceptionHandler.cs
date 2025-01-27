using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

namespace ScreenDrafts.Web.Middleware;
internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
  private readonly ILogger<GlobalExceptionHandler> _logger = logger;

  public async ValueTask<bool> TryHandleAsync(
      HttpContext httpContext,
      Exception exception,
      CancellationToken cancellationToken)
  {
    ExceptionLogging.UnhandledException(_logger, "GlobalExceptionHandler", exception);

    var problemDetails = new ProblemDetails
    {
      Status = StatusCodes.Status500InternalServerError,
      Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
      Title = "Server failure"
    };

    httpContext.Response.StatusCode = problemDetails.Status.Value;

    await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

    return true;
  }
}
