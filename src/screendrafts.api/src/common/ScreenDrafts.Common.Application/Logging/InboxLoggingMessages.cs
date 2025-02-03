namespace ScreenDrafts.Common.Application.Logging;
public static class InboxLoggingMessages
{
  private static readonly Action<ILogger, string, Exception?> _beginningToProcessInboxMessages = LoggerMessage.Define<string>(
    LogLevel.Information,
    new EventId(1, nameof(BeginningToProcessInboxMessages)),
    "Beginning to process outbox messages for {ModuleName}");

  private static readonly Action<ILogger, string, Guid, Exception?> _exceptionWhileProcessingInboxMessage = LoggerMessage.Define<string, Guid>(
    LogLevel.Error,
    new EventId(2, nameof(ExceptionWhileProcessingInboxMessage)),
    "Exception while processing outbox message for {ModuleName} with id {MessageId}");

  private static readonly Action<ILogger, string, Exception?> _completedProcessingInboxMessages = LoggerMessage.Define<string>(
    LogLevel.Information,
    new EventId(3, nameof(CompletedProcessingInboxMessages)),
    "Completed processing outbox messages for {ModuleName}");

  public static void BeginningToProcessInboxMessages(ILogger logger, string moduleName) =>
    _beginningToProcessInboxMessages(logger, moduleName, null);

  public static void ExceptionWhileProcessingInboxMessage(ILogger logger, string moduleName, Guid messageId) =>
    _exceptionWhileProcessingInboxMessage(logger, moduleName, messageId, null);

  public static void CompletedProcessingInboxMessages(ILogger logger, string moduleName) =>
    _completedProcessingInboxMessages(logger, moduleName, null);
}
