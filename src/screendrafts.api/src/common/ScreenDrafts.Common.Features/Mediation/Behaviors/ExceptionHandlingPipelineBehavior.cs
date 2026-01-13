namespace ScreenDrafts.Common.Features.Mediation.Behaviors;

internal sealed class ExceptionHandlingPipelineBehavior<TRequest, TResponse>(
  ILogger<ExceptionHandlingPipelineBehavior<TRequest, TResponse>> logger)
  : IPipelineBehavior<TRequest, TResponse>
  where TRequest : class
{
  private readonly ILogger<ExceptionHandlingPipelineBehavior<TRequest, TResponse>> _logger = logger;

  public async Task<TResponse> Handle(
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken)
  {
    try
    {
      return await next(cancellationToken);
    }
    catch (Exception ex)
    {
      var requestName = typeof(TRequest).Name;

     BehaviorMessages.UnhandledException(_logger, requestName);

      throw new ScreenDraftsException(requestName, innerException: ex);
    }
  }
}
