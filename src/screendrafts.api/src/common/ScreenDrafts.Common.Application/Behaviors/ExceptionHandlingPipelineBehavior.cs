namespace ScreenDrafts.Common.Application.Behaviors;

internal sealed partial class ExceptionHandlingPipelineBehavior<TRequest, TResponse>(
  ILogger<ExceptionHandlingPipelineBehavior<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse>
  where TRequest : class
{
  private readonly ILogger<ExceptionHandlingPipelineBehavior<TRequest, TResponse>> _logger = logger;

  public async Task<TResponse> Handle(
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken
  )
  {
    try
    {
      return await next(cancellationToken);
    }
#pragma warning disable CA1031 // Intentional — this is the last-resort
    // safety net wrapping every MediatR request in the app. Anything
    // unexpected gets logged with request context here, then re-thrown
    // (not swallowed) so GlobalExceptionHandler still turns it into a
    // proper 500 for the client.
    catch (Exception ex)
    {
      var requestName = typeof(TRequest).Name;

      UnhandledException(_logger, requestName, ex);

      throw;
    }
#pragma warning restore CA1031
  }

  [LoggerMessage(
    EventId = 4,
    Level = LogLevel.Error,
    Message = "Unhandled exception for {RequestName}"
  )]
  private static partial void UnhandledException(
    ILogger logger,
    string requestName,
    Exception exception
  );
}
