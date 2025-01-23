namespace ScreenDrafts.Modules.Drafts.Domain.Drafts;

public sealed class DraftType(string name, int value)
  : SmartEnum<DraftType>(name, value)
{
  public static readonly DraftType Regular = new(nameof(Regular), 0);
  public static readonly DraftType Expanded = new(nameof(Expanded), 1);
}
