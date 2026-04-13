namespace ScreenDrafts.Modules.Communications.Domain.Email;

public interface IEmailService
{
  Task SendAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default);
}
