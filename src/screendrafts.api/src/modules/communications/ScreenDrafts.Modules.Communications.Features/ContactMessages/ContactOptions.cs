namespace ScreenDrafts.Modules.Communications.Features.ContactMessages;

internal sealed class ContactOptions
{
  public const string SectionName = "Communications:Contact";

  /// <summary>
  /// Where contact-form messages are emailed. A public address (Cloudflare Email
  /// Routing forwards it), so it lives in appsettings rather than a secret.
  /// </summary>
  public string? RecipientAddress { get; set; } = "support@screen-drafts.com";
}
