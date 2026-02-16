namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Enums;

public sealed class DraftPartStatus(string name, int value)
  : SmartEnum<DraftPartStatus>(name, value)
{
  public static readonly DraftPartStatus Created = new(nameof(Created), 0);
  public static readonly DraftPartStatus InProgress = new(nameof(InProgress), 2);
  public static readonly DraftPartStatus Completed = new(nameof(Completed), 3);
  public static readonly DraftPartStatus Cancelled = new(nameof(Cancelled), 4);
}
