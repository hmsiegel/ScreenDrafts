namespace ScreenDrafts.Modules.Reporting.Features.Drafts.QueryStats;

internal sealed record QueryStatsQuery : IQuery<QueryStatsResponse>
{
  public required string Metric { get; init; }
  public required string GroupBy { get; init; }
  public IReadOnlyList<string>? Series { get; init; }
  public IReadOnlyList<string>? DraftTypes { get; init; }
  public int? EpisodeFrom { get; init; }
  public int? EpisodeTo { get; init; }
  public int? MinAppearances { get; init; }
  public bool Ascending { get; init; }
  public int? Limit { get; init; }
  public required bool IncludeAll { get; init; }
}
