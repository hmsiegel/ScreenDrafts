namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetRecordBook;

internal static class RecordRanker
{
  public const string Count = "count";
  public const string Ratio = "ratio";
  public const string Percent = "percent";

  /// <summary>
  /// Finds the best value among the candidates and returns every candidate that shares it.
  /// Returns null when there is nothing to rank, or when a "most" record would be zero.
  /// </summary>
  public static RecordItem? Build<T>(
    string code,
    string label,
    string format,
    IEnumerable<T> candidates,
    Func<T, decimal> metric,
    bool highest,
    Func<T, RecordHolder> holder,
    string? qualifier = null
  )
  {
    ArgumentNullException.ThrowIfNull(candidates);
    ArgumentNullException.ThrowIfNull(metric);
    ArgumentNullException.ThrowIfNull(holder);

    var scored = candidates.Select(c => (Item: c, Value: Math.Round(metric(c), 4))).ToList();

    if (scored.Count == 0)
    {
      return null;
    }

    var best = highest ? scored.Max(s => s.Value) : scored.Min(s => s.Value);

    if (highest && best <= 0)
    {
      return null;
    }

    return new RecordItem
    {
      Code = code,
      Label = label,
      Format = format,
      Value = best,
      Qualifier = qualifier,
      Holders =
      [
        .. scored
          .Where(s => s.Value == best)
          .Select(s => holder(s.Item))
          .OrderBy(h => h.Name, StringComparer.OrdinalIgnoreCase),
      ],
    };
  }

  /// <summary>One record per minimum-appearance tier (for example 5, 10, 15 and 20 drafts).</summary>
  public static IEnumerable<RecordItem> BuildTiered<T>(
    string code,
    string label,
    string format,
    IEnumerable<T> candidates,
    Func<T, int> appearances,
    Func<T, decimal> metric,
    bool highest,
    Func<T, RecordHolder> holder,
    IEnumerable<int> tiers
  )
  {
    ArgumentNullException.ThrowIfNull(candidates);
    ArgumentNullException.ThrowIfNull(appearances);
    ArgumentNullException.ThrowIfNull(metric);
    ArgumentNullException.ThrowIfNull(holder);
    ArgumentNullException.ThrowIfNull(tiers);

    var all = candidates.ToList();

    return tiers
      .Select(tier =>
        Build(
          code: $"{code}.min{tier}",
          label: label,
          format: format,
          candidates: all.Where(c => appearances(c) >= tier),
          metric: metric,
          highest: highest,
          holder: holder,
          qualifier: $"{tier}+ drafts"
        )
      )
      .OfType<RecordItem>()
      .ToList();
  }
}
