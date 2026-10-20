namespace ScreenDrafts.Modules.Communications.IntegrationTests.Doubles;

internal sealed class FailingEmailService : IEmailService
{
  public bool AllowPlaceholderRecipients { get; }

  public Task SendAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default) =>
    throw new InvalidOperationException("SMTP down");
}
