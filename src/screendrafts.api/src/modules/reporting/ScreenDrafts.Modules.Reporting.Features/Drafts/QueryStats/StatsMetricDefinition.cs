namespace ScreenDrafts.Modules.Reporting.Features.Drafts.QueryStats;

internal sealed record StatsMetricDefinition(
  string Code,
  string Label,
  string Format,
  string Description,
  IReadOnlyList<string> GroupBys);
