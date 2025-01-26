using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace ScreenDrafts.Web.Logging;

internal static class ExceptionLogging
{

  private static readonly Action<ILogger, string, Exception?> _unhandledException = LoggerMessage.Define<string>(
    LogLevel.Error,
    new EventId(1, nameof(UnhandledException)),
    "Unhandled exception for {RequestName}");

  public static void UnhandledException(ILogger logger, string requestName, Exception exception) => _unhandledException(logger, requestName, exception);
}
