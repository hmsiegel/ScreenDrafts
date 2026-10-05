namespace ScreenDrafts.Modules.Reporting.Features.Drafts.QueryStats;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Dapper.")]
internal sealed class QueryVetoRow
{
  public Guid VetoId { get; set; }
  public Guid PickId { get; set; }

  /// <summary>0 = drafter, 1 = team, 2 = community.</summary>
  public int IssuedByKind { get; set; }
  public Guid IssuedByIdValue { get; set; }
  public string? IssuedByPublicId { get; set; }
  public string IssuedByName { get; set; } = string.Empty;
  public bool IsOverridden { get; set; }
  public bool IsSelfVeto { get; set; }
}
