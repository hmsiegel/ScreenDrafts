namespace ScreenDrafts.Domain.DraftAggregate.Enums;
public class DraftType : SmartEnum<DraftType>
{
    public DraftType(string name, int value)
        : base(name, value)
    {
    }

    public static readonly DraftType Regular = new(nameof(Regular), 1);
    public static readonly DraftType MiniMega = new(nameof(MiniMega), 2);
    public static readonly DraftType Mega = new(nameof(Mega), 3);
    public static readonly DraftType MiniSuper = new(nameof(MiniSuper), 4);
    public static readonly DraftType Super = new(nameof(Super), 5);
}
