namespace ScreenDrafts.Common.Features.Abstractions.Logging;
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

  private static readonly Action<ILogger, string, Guid, Exception?> _emptyContent = LoggerMessage.Define<string, Guid>(
    LogLevel.Warning,
    new EventId(4, nameof(EmptyContent)),
    "Inbox message {Id} in module {ModuleName} has empty content. Skipping...");

  private static readonly Action<ILogger, Guid, string, string, Exception?> _failedToDeserialize = LoggerMessage.Define<Guid, string, string>(
    LogLevel.Error,
    new EventId(5, nameof(FailedToDeserialize)),
    "Failed to deserialize inbox message {Id} in module {ModuleName}: {ExceptionMessage}");

  private static readonly Action<ILogger, Guid, string, Exception?> _markingAsFailed = LoggerMessage.Define<Guid, string>(
    LogLevel.Warning,
    new EventId(6, nameof(MarkingAsFailed)),
    "Inbox message {Id} in module {ModuleName} could not be deserialized. Marking as failed...");

  private static readonly Action<ILogger, int, string, Exception?> _deletedInboxMessages = LoggerMessage.Define<int, string>(
    LogLevel.Warning,
    new EventId(7, nameof(DeletedInboxMessages)),
    "Deleted {Count} invalid inbox messages in module {ModuleName}.");
  

  public static void BeginningToProcessInboxMessages(ILogger logger, string moduleName) =>
    _beginningToProcessInboxMessages(logger, moduleName, null);

  public static void ExceptionWhileProcessingInboxMessage(ILogger logger, string moduleName, Guid messageId) =>
    _exceptionWhileProcessingInboxMessage(logger, moduleName, messageId, null);

  public static void CompletedProcessingInboxMessages(ILogger logger, string moduleName) =>
    _completedProcessingInboxMessages(logger, moduleName, null);

  public static void EmptyContent(ILogger logger, string moduleName, Guid messageId) =>
    _emptyContent(logger, moduleName, messageId, null);

  public static void FailedToDeserialize(ILogger logger, Guid messageId, string moduleName, string exceptionMessage, Exception exception) =>
    _failedToDeserialize(logger, messageId, moduleName, exceptionMessage, exception);

  public static void MarkingAsFailed(ILogger logger, Guid messageId, string moduleName) =>
    _markingAsFailed(logger, messageId, moduleName, null);

  public static void DeletedInboxMessages(ILogger logger, int messageCount, string moduleName) =>
    _deletedInboxMessages(logger, messageCount, moduleName, null);
}
