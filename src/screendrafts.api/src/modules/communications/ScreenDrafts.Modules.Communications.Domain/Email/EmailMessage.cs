namespace ScreenDrafts.Modules.Communications.Domain.Email;

public sealed record EmailMessage
{
  /// <summary>
  /// Content-ID of the inline Screen Drafts logo. Templates reference it as
  /// <c>cid:screendrafts-logo</c>; the email service attaches the image to the message.
  /// </summary>
  public const string LogoContentId = "screendrafts-logo";

  public required string ToAddress { get; init; }
  public required string ToName { get; init; }
  public required string Subject { get; init; }
  public required string HtmlBody { get; init; }
}
