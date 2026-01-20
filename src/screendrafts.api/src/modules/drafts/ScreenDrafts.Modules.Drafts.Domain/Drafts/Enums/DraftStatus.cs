namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Enums;

public sealed class DraftStatus(string name, int value)
  : SmartEnum<DraftStatus>(name, value)
{
  public static readonly DraftStatus Created = new(nameof(Created), 0);
  public static readonly DraftStatus InProgress = new(nameof(InProgress), 2);
  public static readonly DraftStatus Paused = new(nameof(Paused), 3); 
  public static readonly DraftStatus Completed = new(nameof(Completed), 4);
  public static readonly DraftStatus Cancelled = new(nameof(Cancelled), 5);
}
