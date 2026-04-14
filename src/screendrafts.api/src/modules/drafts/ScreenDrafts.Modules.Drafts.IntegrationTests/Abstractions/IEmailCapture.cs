using ScreenDrafts.Modules.Communications.Domain.Email;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Abstractions;

/// <summary>
/// Captures outbound emails for test assertions.
/// Registered as a singleton alongside IEmailService so assertions work across
/// the full outbox → MassTransit → Communications consumer pipeline.
/// </summary>
public interface IEmailCapture
{
  IReadOnlyList<EmailMessage> SentEmails { get; }
  void Clear();
}
