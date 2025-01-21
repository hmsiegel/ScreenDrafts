namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Enums;

public sealed class DraftStatus(string name, int value)
  : SmartEnum<DraftStatus>(name, value)
{
  public static readonly DraftStatus Created = new(nameof(Created), 0);
  public static readonly DraftStatus InProgress = new(nameof(InProgress), 1);
  public static readonly DraftStatus Completed = new(nameof(Completed), 2);
}
