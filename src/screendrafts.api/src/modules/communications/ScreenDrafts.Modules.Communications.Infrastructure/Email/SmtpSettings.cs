namespace ScreenDrafts.Modules.Communications.Infrastructure.Email;

internal sealed class SmtpSettings
{
  public const string SectionName = "Communications:Smtp";

  public string Host { get; set; } = "localhost";
  public int Port { get; set; } = 25;

  /// <summary>
  /// Controls TLS negotiation. Use None for Papercut in development, StartTls or SslOnConnect for production.
  /// Serializes from config ase the enum name, e.g. "Auto", "None", "StartTls", "SslOnConnect".
  /// </summary>
  public SecureSocketOptions SecureSocketOptions { get; set; } = SecureSocketOptions.Auto;

  public string FromAddress { get; set; } = "noreply@screen-drafts.com";
  public string FromName { get; set; } = "Screen Drafts";
  public string? Username { get; set; }
  public string? Password { get; set; }
}
