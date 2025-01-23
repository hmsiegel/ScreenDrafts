namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Enums;

public sealed class DraftType(string name, int value)
  : SmartEnum<DraftType>(name, value)
{
  public static readonly DraftType Regular = new(nameof(Regular), 0);
  public static readonly DraftType MiniMega = new(nameof(MiniMega), 1);
  public static readonly DraftType Mega = new(nameof(Mega), 2);
  public static readonly DraftType Super = new(nameof(Super), 3);
  public static readonly DraftType MiniSuper = new(nameof(MiniSuper), 4);
}
