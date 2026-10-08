namespace ScreenDrafts.Modules.Communications.Infrastructure.Email;

internal sealed partial class SmtpEmailService(
  IOptions<SmtpSettings> smtpSettings,
  ILogger<SmtpEmailService> logger
) : IEmailService
{
  // Matches LogicalName in the .csproj. Embedded (not fetched from the website) so the
  // logo shows even when the recipient's client blocks remote images.
  private const string LogoResourceName = "screendrafts-logo.jpg";

  private static readonly Lazy<byte[]> _logoBytes = new(LoadLogo);

  private readonly SmtpSettings _smtpSettings = smtpSettings.Value;
  private readonly ILogger<SmtpEmailService> _logger = logger;

  public bool AllowPlaceholderRecipients => _smtpSettings.AllowPlaceholderRecipients;

  public async Task SendAsync(
    EmailMessage emailMessage,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(emailMessage);

    // Single choke point: whatever path produced this message, a placeholder address
    // (users not yet migrated to a real email) never reaches the SMTP server.
    if (!AllowPlaceholderRecipients && PlaceholderEmail.IsPlaceholder(emailMessage.ToAddress))
    {
      LogSkippedPlaceholderRecipient(_logger, emailMessage.ToAddress, emailMessage.Subject);
      return;
    }

    using var mimeMessage = BuildMimeMessage(emailMessage);

    using var client = new SmtpClient();

    await client.ConnectAsync(
      host: _smtpSettings.Host,
      port: _smtpSettings.Port,
      options: _smtpSettings.SecureSocketOptions,
      cancellationToken: cancellationToken
    );

    if (!string.IsNullOrWhiteSpace(_smtpSettings.Username))
    {
      await client.AuthenticateAsync(
        userName: _smtpSettings.Username,
        password: _smtpSettings.Password!,
        cancellationToken: cancellationToken
      );
    }

    await client.SendAsync(mimeMessage, cancellationToken);
    await client.DisconnectAsync(true, cancellationToken);
  }

  // Synchronous on purpose: building the message is CPU-only, and keeping it out of the
  // async SendAsync avoids CA1849 on MimeKit's sync-only-in-practice LinkedResources.Add.
  private MimeMessage BuildMimeMessage(EmailMessage emailMessage)
  {
    var mimeMessage = new MimeMessage();

    mimeMessage.From.Add(new MailboxAddress(_smtpSettings.FromName, _smtpSettings.FromAddress));
    mimeMessage.To.Add(new MailboxAddress(emailMessage.ToName, emailMessage.ToAddress));
    mimeMessage.Subject = emailMessage.Subject;

    var bodyBuilder = new BodyBuilder { HtmlBody = emailMessage.HtmlBody };

    if (
      emailMessage.HtmlBody.Contains($"cid:{EmailMessage.LogoContentId}", StringComparison.Ordinal)
    )
    {
      var logo = bodyBuilder.LinkedResources.Add(
        LogoResourceName,
        _logoBytes.Value,
        new MimeKit.ContentType("image", "jpeg")
      );
      logo.ContentId = EmailMessage.LogoContentId;
    }

    mimeMessage.Body = bodyBuilder.ToMessageBody();
    return mimeMessage;
  }

  private static byte[] LoadLogo()
  {
    using var stream =
      typeof(SmtpEmailService).Assembly.GetManifestResourceStream(LogoResourceName)
      ?? throw new InvalidOperationException(
        $"Embedded resource '{LogoResourceName}' was not found."
      );

    using var memory = new MemoryStream();
    stream.CopyTo(memory);
    return memory.ToArray();
  }

  [LoggerMessage(
    Level = LogLevel.Warning,
    Message = "Skipped email '{Subject}' to placeholder address {ToAddress}."
  )]
  private static partial void LogSkippedPlaceholderRecipient(
    ILogger<SmtpEmailService> logger,
    string toAddress,
    string subject
  );
}
