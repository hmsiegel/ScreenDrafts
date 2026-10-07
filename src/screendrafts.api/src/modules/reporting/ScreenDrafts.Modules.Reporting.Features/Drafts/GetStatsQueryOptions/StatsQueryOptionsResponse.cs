namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetStatsQueryOptions;

internal sealed record StatsQueryOptionsResponse
{
  /// <summary>True when the caller may include Patreon and Speed drafts (the includeAll toggle takes effect).</summary>
  public bool CanIncludeAll { get; init; }

  public IReadOnlyList<StatsMetricOption> Metrics { get; init; } = [];
  public IReadOnlyList<StatsGroupByOption> GroupBys { get; init; } = [];
  public IReadOnlyList<string> Series { get; init; } = [];
  public IReadOnlyList<string> DraftTypes { get; init; } = [];

  /// <summary>Lowest and highest main-feed episode numbers, for the episode range filter.</summary>
  public int? MinEpisode { get; init; }
  public int? MaxEpisode { get; init; }
}
