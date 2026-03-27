using Ardalis.SmartEnum;

namespace ScreenDrafts.Modules.Reporting.Domain.Movies;

public sealed class MovieHonorific(string name, int value)
  : SmartEnum<MovieHonorific>(name, value)
{
  public static readonly MovieHonorific None = new(nameof(None), 0);

  /// <summary>
  /// 2 canonical appearances
  /// </summary>
  public static readonly MovieHonorific MarqueeOfFame = new(nameof(MarqueeOfFame), 1);

  /// <summary>
  /// 3 canonical appearances
  /// </summary>
  public static readonly MovieHonorific HatTrick = new(nameof(HatTrick), 2);

  /// <summary>
  /// 4 canonical appearances
  /// </summary>
  public static readonly MovieHonorific GrandSlam = new(nameof(GrandSlam), 3);

  /// <summary>
  /// 5+ canoonical appearances
  /// </summary>
  public static readonly MovieHonorific HighFive = new(nameof(HighFive), 4);
  public static MovieHonorific FromAppearanceCount(int count)
    => count switch
    {
      >= 5 => HighFive,
      4 => GrandSlam,
      3 => HatTrick,
      2 => MarqueeOfFame,
      _ => None
    };
}
