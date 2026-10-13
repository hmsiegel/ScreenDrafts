namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetTitleHonorifics;

/// <summary>
/// Decides which appearance, if any, earns a title a level. Appearances must already be in release order.
/// </summary>
internal static class TitleHonorificJoin
{
  public const int FirstPlace = 1;
  public const int CycleSize = 4;
  private const int UnifiedNo1Finishes = 2;

  /// <summary>Index of the appearance that earns the level, or null when the title does not hold it.</summary>
  public static int? JoinIndex(IReadOnlyList<TitleAppearanceRow> ordered, TitleHonorificLevel level) =>
    level.Kind switch
    {
      TitleHonorificKind.UnifiedNo1 => UnifiedNo1Index(ordered),
      TitleHonorificKind.TheCycle => CycleIndex(ordered),
      _ => CountIndex(ordered, level.MinAppearances),
    };

  /// <summary>The places from 1 to 4 the title has finished at, lowest first. Finishes below No. 4 are ignored.</summary>
  public static IReadOnlyList<int> CyclePlacesHeld(IEnumerable<TitleAppearanceRow> appearances) =>
    [
      .. appearances
        .Select(a => a.Position)
        .Where(p => p is >= FirstPlace and <= CycleSize)
        .Distinct()
        .Order(),
    ];

  /// <summary>The one place of 1 to 4 a title one short of The Cycle has not finished at.</summary>
  public static int MissingCyclePlace(IReadOnlyList<int> held) =>
    Enumerable.Range(FirstPlace, CycleSize).Except(held).Single();

  private static int? CountIndex(IReadOnlyList<TitleAppearanceRow> ordered, int minimum) =>
    ordered.Count >= minimum ? minimum - 1 : null;

  private static int? UnifiedNo1Index(IReadOnlyList<TitleAppearanceRow> ordered)
  {
    var finishes = 0;

    for (var i = 0; i < ordered.Count; i++)
    {
      if (ordered[i].Position != FirstPlace)
      {
        continue;
      }

      finishes++;

      if (finishes == UnifiedNo1Finishes)
      {
        return i;
      }
    }

    return null;
  }

  private static int? CycleIndex(IReadOnlyList<TitleAppearanceRow> ordered)
  {
    var seen = new HashSet<int>();

    for (var i = 0; i < ordered.Count; i++)
    {
      if (ordered[i].Position is >= FirstPlace and <= CycleSize)
      {
        seen.Add(ordered[i].Position);
      }

      if (seen.Count == CycleSize)
      {
        return i;
      }
    }

    return null;
  }
}
