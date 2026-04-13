namespace ScreenDrafts.Modules.Communications.IntegrationTests.Doubles;

internal sealed class RecordingEmailService : IEmailService
{
  private readonly List<EmailMessage> _sentEmails = [];

  public IReadOnlyList<EmailMessage> SentEmails => _sentEmails;

  public Task SendAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default)
  {
    _sentEmails.Add(emailMessage);
    return Task.CompletedTask;
  }
}
