namespace ScreenDrafts.Modules.Communications.Infrastructure.Email;

internal sealed class SmtpEmailService(IOptions<SmtpSettings> smtpSettings) : IEmailService
{
  private readonly SmtpSettings _smtpSettings = smtpSettings.Value;
  public async Task SendAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default)
  {
    using var mimeMessage = new MimeMessage();

    mimeMessage.From.Add(new MailboxAddress(_smtpSettings.FromName, _smtpSettings.FromAddress));
    mimeMessage.To.Add(new MailboxAddress(emailMessage.ToName, emailMessage.ToAddress));
    mimeMessage.Subject = emailMessage.Subject;
    mimeMessage.Body = new TextPart("html")
    {
      Text = emailMessage.HtmlBody
    };

    using var client = new SmtpClient();

    await client.ConnectAsync(
      host: _smtpSettings.Host,
      port: _smtpSettings.Port,
      options: _smtpSettings.SecureSocketOptions,
      cancellationToken: cancellationToken);

    if (!string.IsNullOrEmpty(_smtpSettings.Username))
    {
      await client.AuthenticateAsync(
        userName: _smtpSettings.Username,
        password: _smtpSettings.Password,
        cancellationToken: cancellationToken);
    }

    await client.SendAsync(mimeMessage, cancellationToken);
    await client.DisconnectAsync(true, cancellationToken);
  }
}
