namespace ScreenDrafts.Common.Features.Abstractions.Logging;

public static class IdentityMessages
{
  private static readonly Action<ILogger, string, Exception?> _userRegistrationFailed = LoggerMessage.Define<string>(
    LogLevel.Error,
    new EventId(1, nameof(UserRegistrationFailed)),
    "User registration failed for {Email}");

  public static void UserRegistrationFailed(ILogger logger, string email) => _userRegistrationFailed(logger, email, null);
}
