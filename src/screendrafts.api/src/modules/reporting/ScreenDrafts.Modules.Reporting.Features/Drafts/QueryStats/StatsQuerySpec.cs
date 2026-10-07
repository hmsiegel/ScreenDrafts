namespace ScreenDrafts.Modules.Reporting.Features.Drafts.QueryStats;

/// <summary>A validated query: known metric and group-by, a compatible pairing, and bounded filters.</summary>
internal sealed record StatsQuerySpec
{
  public required string Metric { get; init; }
  public required string GroupBy { get; init; }
  public required HashSet<string> Series { get; init; }
  public required HashSet<string> DraftTypes { get; init; }
  public int? EpisodeFrom { get; init; }
  public int? EpisodeTo { get; init; }
  public int? MinAppearances { get; init; }
  public required bool Ascending { get; init; }
  public required int Limit { get; init; }
}
