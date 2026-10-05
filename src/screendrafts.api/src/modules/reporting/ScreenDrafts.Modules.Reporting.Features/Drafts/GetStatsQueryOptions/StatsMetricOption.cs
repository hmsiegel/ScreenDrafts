namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetStatsQueryOptions;

internal sealed record StatsMetricOption(
  string Code,
  string Label,
  string Format,
  string Description,
  IReadOnlyList<string> GroupBys);
