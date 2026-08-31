namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.Enums;

public sealed class GuestDraftStatus(string name, int value)
  : SmartEnum<GuestDraftStatus>(name, value)
{
  public static readonly GuestDraftStatus Created = new(nameof(Created), 0);
  public static readonly GuestDraftStatus InProgress = new(nameof(InProgress), 1);
  public static readonly GuestDraftStatus Paused = new(nameof(Paused), 2);
  public static readonly GuestDraftStatus Completed = new(nameof(Completed), 3);
  public static readonly GuestDraftStatus Cancelled = new(nameof(Cancelled), 4);
}
