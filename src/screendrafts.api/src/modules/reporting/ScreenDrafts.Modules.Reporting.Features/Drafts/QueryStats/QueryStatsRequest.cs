namespace ScreenDrafts.Modules.Reporting.Features.Drafts.QueryStats;

internal sealed record QueryStatsRequest
{
  /// <summary>One of the metric codes from GET /stats/query/options.</summary>
  public string Metric { get; init; } = string.Empty;

  /// <summary>drafter, draft, series, draftType or title. Not every metric supports every group-by.</summary>
  public string GroupBy { get; init; } = string.Empty;

  public IReadOnlyList<string>? Series { get; init; }
  public IReadOnlyList<string>? DraftTypes { get; init; }

  /// <summary>Inclusive range on the site's main-feed episode number. A draft with no episode number is excluded.</summary>
  public int? EpisodeFrom { get; init; }
  public int? EpisodeTo { get; init; }

  /// <summary>Drafter grouping only: skip drafters with fewer appearances in the filtered drafts.</summary>
  public int? MinAppearances { get; init; }

  /// <summary>Sort lowest first. Zero-valued groups are included only when ascending.</summary>
  public bool Ascending { get; init; }

  /// <summary>Rows to return, 1 to 100. Defaults to 25.</summary>
  public int? Limit { get; init; }

  /// <summary>Include Patreon and Speed drafts. Only honored for Patreon members.</summary>
  public bool IncludeAll { get; init; }
}
