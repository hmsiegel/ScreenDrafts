namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.Enums;

public sealed class GuestDraftType(string name, int value) : SmartEnum<GuestDraftType>(name, value)
{
  public static readonly GuestDraftType Standard = new(nameof(Standard), 0);
  public static readonly GuestDraftType MiniMega = new(nameof(MiniMega), 1);
  public static readonly GuestDraftType Mega = new(nameof(Mega), 2);
  public static readonly GuestDraftType Super = new(nameof(Super), 3);
  public static readonly GuestDraftType MiniSuper = new(nameof(MiniSuper), 4);
}
