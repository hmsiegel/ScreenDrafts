namespace ScreenDrafts.Modules.Reporting.Features.Drafts.QueryStats;

internal sealed record QueryDataset
{
  public required IReadOnlyList<QueryPickRow> Picks { get; init; }
  public required IReadOnlyList<QueryCreditRow> Credits { get; init; }
  public required IReadOnlyList<QueryVetoRow> Vetoes { get; init; }
}
