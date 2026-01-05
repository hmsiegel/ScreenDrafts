namespace ScreenDrafts.Common.Application.Behaviors;

internal sealed class RequestLoggingPipelineBehavior<TRequest, TResponse>
  (ILogger<RequestLoggingPipelineBehavior<TRequest, TResponse>> logger)
  : IPipelineBehavior<TRequest, TResponse>
  where TRequest : class
  where TResponse : Result
{
  private readonly ILogger<RequestLoggingPipelineBehavior<TRequest, TResponse>> _logger = logger;

  public async Task<TResponse> Handle(
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken)
  {
    var moduleName = GetModuleName(typeof(TRequest).FullName!);
    var requsestName = typeof(TRequest).Name;

    Activity.Current?.SetTag("request.module", moduleName);
    Activity.Current?.SetTag("request.name", requsestName);

    using (LogContext.PushProperty("Module", moduleName))
    {
      BehaviorMessages.ProcessingRequest(_logger, requsestName);

      var result = await next(cancellationToken);

      if (result.IsSuccess)
      {
        BehaviorMessages.RequestProcessedSuccessfully(_logger, requsestName);
      }
      else
      {
        using (LogContext.PushProperty("Error", result.Errors, true))
        {
          BehaviorMessages.RequestProcessedWithError(_logger, requsestName);
        }
      }

      return result;
    }
  }

  private static string GetModuleName(string requestName) => requestName.Split('.')[2];
}
