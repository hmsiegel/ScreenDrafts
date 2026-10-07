namespace ScreenDrafts.Modules.Reporting.Domain.Drafts;

public static class StatsQueryErrors
{
  public static SDError UnknownMetric(string metric) =>
    SDError.Problem("StatsQuery.UnknownMetric", $"Unknown metric '{metric}'.");

  public static SDError UnknownGroupBy(string groupBy) =>
    SDError.Problem("StatsQuery.UnknownGroupBy", $"Unknown group-by '{groupBy}'.");

  public static SDError IncompatibleGroupBy(string metric, string groupBy) =>
    SDError.Problem(
      "StatsQuery.IncompatibleGroupBy",
      $"The metric '{metric}' cannot be grouped by '{groupBy}'."
    );

  public static readonly SDError InvalidLimit = SDError.Problem(
    "StatsQuery.InvalidLimit",
    "Limit must be between 1 and 100."
  );

  public static readonly SDError MinAppearancesNeedsDrafterGrouping = SDError.Problem(
    "StatsQuery.MinAppearancesNeedsDrafterGrouping",
    "MinAppearances only applies when grouping by drafter."
  );

  public static readonly SDError InvalidMinAppearances = SDError.Problem(
    "StatsQuery.InvalidMinAppearances",
    "MinAppearances must be between 1 and 500."
  );

  public static readonly SDError InvalidEpisodeRange = SDError.Problem(
    "StatsQuery.InvalidEpisodeRange",
    "EpisodeFrom must not be greater than EpisodeTo."
  );

  public static readonly SDError TooManyFilterValues = SDError.Problem(
    "StatsQuery.TooManyFilterValues",
    "A filter list can hold at most 50 values."
  );
}
