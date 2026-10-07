namespace ScreenDrafts.Modules.Reporting.Features.Drafts.QueryStats;

internal sealed record StatsQueryResult(IReadOnlyList<QueryStatsRow> Rows, int TotalGroups);
