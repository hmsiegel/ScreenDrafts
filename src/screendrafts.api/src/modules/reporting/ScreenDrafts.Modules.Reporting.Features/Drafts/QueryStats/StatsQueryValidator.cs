namespace ScreenDrafts.Modules.Reporting.Features.Drafts.QueryStats;

internal static class StatsQueryValidator
{
  public const int DefaultLimit = 25;
  public const int MaxLimit = 100;
  private const int MaxFilterValues = 50;
  private const int MaxMinAppearances = 500;

  public static StatsQueryValidation Validate(QueryStatsQuery query)
  {
    ArgumentNullException.ThrowIfNull(query);

    var metric = StatsMetrics.Find(query.Metric);

    if (metric is null)
    {
      return StatsQueryValidation.Fail(StatsQueryErrors.UnknownMetric(query.Metric));
    }

    if (!StatsGroupBys.IsKnown(query.GroupBy))
    {
      return StatsQueryValidation.Fail(StatsQueryErrors.UnknownGroupBy(query.GroupBy));
    }

    if (!metric.GroupBys.Contains(query.GroupBy))
    {
      return StatsQueryValidation.Fail(
        StatsQueryErrors.IncompatibleGroupBy(query.Metric, query.GroupBy)
      );
    }

    var limit = query.Limit ?? DefaultLimit;

    if (limit is < 1 or > MaxLimit)
    {
      return StatsQueryValidation.Fail(StatsQueryErrors.InvalidLimit);
    }

    if (query.MinAppearances is { } minAppearances)
    {
      if (query.GroupBy != StatsGroupBys.Drafter)
      {
        return StatsQueryValidation.Fail(StatsQueryErrors.MinAppearancesNeedsDrafterGrouping);
      }

      if (minAppearances is < 1 or > MaxMinAppearances)
      {
        return StatsQueryValidation.Fail(StatsQueryErrors.InvalidMinAppearances);
      }
    }

    if (query.EpisodeFrom > query.EpisodeTo)
    {
      return StatsQueryValidation.Fail(StatsQueryErrors.InvalidEpisodeRange);
    }

    var series = query.Series ?? [];
    var draftTypes = query.DraftTypes ?? [];

    if (series.Count > MaxFilterValues || draftTypes.Count > MaxFilterValues)
    {
      return StatsQueryValidation.Fail(StatsQueryErrors.TooManyFilterValues);
    }

    return StatsQueryValidation.Ok(
      new StatsQuerySpec
      {
        Metric = query.Metric,
        GroupBy = query.GroupBy,
        Series = new HashSet<string>(series, StringComparer.OrdinalIgnoreCase),
        DraftTypes = new HashSet<string>(draftTypes, StringComparer.OrdinalIgnoreCase),
        EpisodeFrom = query.EpisodeFrom,
        EpisodeTo = query.EpisodeTo,
        MinAppearances = query.MinAppearances,
        Ascending = query.Ascending,
        Limit = limit,
      }
    );
  }
}
