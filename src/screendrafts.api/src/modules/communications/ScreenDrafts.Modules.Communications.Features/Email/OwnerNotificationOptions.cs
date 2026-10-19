namespace ScreenDrafts.Modules.Communications.Features.Email;

internal sealed class OwnerNotificationOptions
{
  public const string SectionName = "Communications:OwnerNotification";

  /// <summary>
  /// Where owner notifications (e.g. "new user registered") are sent. Blank or
  /// missing disables them. Supply via Vault in dev and a Docker secret file in
  /// prod (Communications__OwnerNotification__Address) — never appsettings.
  /// </summary>
  public string? Address { get; set; }
}
