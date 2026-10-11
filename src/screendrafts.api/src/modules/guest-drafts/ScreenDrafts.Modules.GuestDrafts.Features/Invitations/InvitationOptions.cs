namespace ScreenDrafts.Modules.GuestDrafts.Features.Invitations;

internal sealed class InvitationOptions
{
  public const string SectionName = "GuestDrafts:Invitations";

  /// <summary>Absolute URL of the web app's register page, e.g. https://screendrafts.com/register.</summary>
  public string RegisterBaseUrl { get; set; } = string.Empty;
}
