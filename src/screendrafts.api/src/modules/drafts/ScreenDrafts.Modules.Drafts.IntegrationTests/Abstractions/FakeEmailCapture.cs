using ScreenDrafts.Modules.Communications.Domain.Email;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Abstractions;

/// <summary>
/// In-memory implementation that implements both <see cref="IEmailService"/> (for DI registration)
/// and <see cref="IEmailCapture"/> (for test assertions). Registered as a singleton so that the
/// Communications module's consumers write to the same instance the test reads from.
/// </summary>
public sealed class FakeEmailCapture : IEmailService, IEmailCapture
{
  private readonly List<EmailMessage> _sent = [];

  public IReadOnlyList<EmailMessage> SentEmails => _sent.AsReadOnly();

  public Task SendAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default)
  {
    _sent.Add(emailMessage);
    return Task.CompletedTask;
  }

  public void Clear() => _sent.Clear();
}
