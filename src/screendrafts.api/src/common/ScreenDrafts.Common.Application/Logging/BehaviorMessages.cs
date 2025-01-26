using Microsoft.Extensions.Logging;

namespace ScreenDrafts.Common.Application.Logging;
public static class BehaviorMessages
{
  private static readonly Action<ILogger, string, Exception?> _processingRequest = LoggerMessage.Define<string>(
    LogLevel.Information,
    new EventId(1, nameof(ProcessingRequest)),
    "Processing request {RequestName}");

  private static readonly Action<ILogger, string, Exception?> _requestProcessedSuccessfully = LoggerMessage.Define<string>(
    LogLevel.Information,
    new EventId(2, nameof(RequestProcessedSuccessfully)),
    "Completed request {RequestName} successfully");

  private static readonly Action<ILogger, string, Exception?> _requestProcessedWithError = LoggerMessage.Define<string>(
    LogLevel.Error,
    new EventId(3, nameof(RequestProcessedWithError)),
    "Completed request {RequestName} with error");

  public static void ProcessingRequest(ILogger logger, string requestName) => _processingRequest(logger, requestName, null);

  public static void RequestProcessedSuccessfully(ILogger logger, string requestName) => _requestProcessedSuccessfully(logger, requestName, null);

  public static void RequestProcessedWithError(ILogger logger, string requestName) => _requestProcessedWithError(logger, requestName, null);
}
