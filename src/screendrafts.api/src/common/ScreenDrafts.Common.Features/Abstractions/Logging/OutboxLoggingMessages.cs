namespace ScreenDrafts.Common.Features.Abstractions.Logging;
public static class OutboxLoggingMessages
{
  private static readonly Action<ILogger, string, Exception?> _beginningToProcessOutboxMessages = LoggerMessage.Define<string>(
    LogLevel.Information,
    new EventId(1, nameof(BeginningToProcessOutboxMessages)),
    "Beginning to process outbox messages for {ModuleName}");

  private static readonly Action<ILogger, string, Guid, Exception?> _exceptionWhileProcessingOutboxMessage = LoggerMessage.Define<string, Guid>(
    LogLevel.Error,
    new EventId(2, nameof(ExceptionWhileProcessingOutboxMessage)),
    "Exception while processing outbox message for {ModuleName} with id {MessageId}");

  private static readonly Action<ILogger, string, Exception?> _completedProcessingOutboxMessages = LoggerMessage.Define<string>(
    LogLevel.Information,
    new EventId(3, nameof(CompletedProcessingOutboxMessages)),
    "Completed processing outbox messages for {ModuleName}");

  public static void BeginningToProcessOutboxMessages(ILogger logger, string moduleName) =>
    _beginningToProcessOutboxMessages(logger, moduleName, null);

  public static void ExceptionWhileProcessingOutboxMessage(ILogger logger, string moduleName, Guid messageId) =>
    _exceptionWhileProcessingOutboxMessage(logger, moduleName, messageId, null);

  public static void CompletedProcessingOutboxMessages(ILogger logger, string moduleName) =>
    _completedProcessingOutboxMessages(logger, moduleName, null);
}
