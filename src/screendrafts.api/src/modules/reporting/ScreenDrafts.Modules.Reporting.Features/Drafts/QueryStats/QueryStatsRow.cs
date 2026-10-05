namespace ScreenDrafts.Modules.Reporting.Features.Drafts.QueryStats;

internal sealed record QueryStatsRow
{
  /// <summary>Competition rank: tied rows share a rank.</summary>
  public required int Rank { get; init; }
  public required string Name { get; init; }
  public string? PublicId { get; init; }
  public required decimal Value { get; init; }

  /// <summary>For drafter rows, the number of drafts the value is measured over, such as "12 drafts".</summary>
  public string? Context { get; init; }
}
