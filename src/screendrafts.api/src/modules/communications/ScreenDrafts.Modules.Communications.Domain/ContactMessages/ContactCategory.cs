namespace ScreenDrafts.Modules.Communications.Domain.ContactMessages;

/// <summary>
/// What the sender is contacting us about. Stored as the enum name (string) so the
/// column stays readable in DBeaver. Plain enum rather than a SmartEnum: Communications.Domain
/// has no SmartEnum dependency and this set carries no behavior.
/// </summary>
public enum ContactCategory
{
  Bug,
  Question,
  Feedback,
  AccountHelp,
}
