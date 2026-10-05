namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetStatsQueryOptions;

internal sealed record GetStatsQueryOptionsQuery : IQuery<StatsQueryOptionsResponse>
{
  /// <summary>Patreon members see every series and draft type; everyone else only canonical ones.</summary>
  public required bool IncludeAll { get; init; }
}
