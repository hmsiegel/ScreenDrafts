namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetRecordBook;

/// <summary>One drafter's career totals, rolled up from their per-draft rows.</summary>
internal sealed class DrafterSummary
{
  public Guid Id { get; init; }
  public string PublicId { get; init; } = string.Empty;
  public string Name { get; init; } = string.Empty;

  /// <summary>Distinct drafts with a credited pick. A multi-part draft counts once.</summary>
  public int Appearances { get; init; }

  public int TitlesDrafted { get; init; }
  public int PicksVetoed { get; init; }
  public int PicksSaved { get; init; }
  public int PicksRemovedByCommissioner { get; init; }
  public int No1Landed { get; init; }
  public int No1Vetoed { get; init; }

  public int VetoesUsed { get; init; }
  public int VetoesOverridden { get; init; }
  public int SelfVetoes { get; init; }
  public int No1VetoesUsed { get; init; }
  public int OverridesDeployed { get; init; }

  public int DraftsWithVetoAgainst { get; init; }
  public int DraftsWithCommissionerOverride { get; init; }
  public int CopaceticDrafts { get; init; }

  /// <summary>Longest run of consecutive appearances, in episode order, with no commissioner override.</summary>
  public int LongestRunWithoutCommissionerOverride { get; init; }
}
