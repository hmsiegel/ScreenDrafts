namespace ScreenDrafts.Modules.Reporting.Domain.Drafters;

public sealed class DrafterHonorific(string name, int value)
  : SmartEnum<DrafterHonorific>(name, value)
{
  public static readonly DrafterHonorific None = new(nameof(None), 0);

  /// <summary>
  /// 5 - 9 canonical appearances
  /// </summary>
  public static readonly DrafterHonorific AllStar = new(nameof(AllStar), 1);

  /// <summary>
  /// 10 - 14 appearances
  /// </summary>
  public static readonly DrafterHonorific HallOfFame = new(nameof(HallOfFame), 2);

  /// <summary>
  /// 15 - 19 appearances
  /// </summary>
  public static readonly DrafterHonorific Mvp = new(nameof(Mvp), 3);

  /// <summary>
  /// 20+ appearances
  /// </summary>
  public static readonly DrafterHonorific Legend = new(nameof(Legend), 4);

  public static DrafterHonorific FromAppearanceCount(int count)
    => count switch
    {
      >= 20 => Legend,
      >= 15 => Mvp,
      >= 10 => HallOfFame,
      >= 5 => AllStar,
      _ => None
    };
}
