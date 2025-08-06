namespace ScreenDrafts.Common.Application.Logging;

public static class PeogleLoggingMessages
{
  private static readonly Action<ILogger, Guid, Exception?> _personAlreadyExists = LoggerMessage.Define<Guid>(
    LogLevel.Warning,
    new EventId(1, nameof(PersonAlreadyExists)),
    "The person with Id {PersonId} already exists.");

  public static void PersonAlreadyExists(ILogger logger, Guid userId) => _personAlreadyExists(logger, userId, null);
}
