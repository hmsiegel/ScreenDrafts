namespace ScreenDrafts.Modules.Reporting.Features.Drafts.QueryStats;

internal sealed record QueryStatsResponse
{
  public required string Metric { get; init; }
  public required string MetricLabel { get; init; }
  public required string GroupBy { get; init; }

  /// <summary>"count" or "ratio".</summary>
  public required string Format { get; init; }

  /// <summary>True when Patreon and Speed drafts were included.</summary>
  public required bool IncludesNonCanonical { get; init; }

  public required int TotalGroups { get; init; }
  public required bool Truncated { get; init; }
  public IReadOnlyList<QueryStatsRow> Rows { get; init; } = [];
}
