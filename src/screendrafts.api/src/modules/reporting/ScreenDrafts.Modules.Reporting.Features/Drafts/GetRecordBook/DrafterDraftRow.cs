namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetRecordBook;

/// <summary>
/// One drafter's numbers in one draft. Filled from three queries and merged, so Dapper needs
/// settable properties. Appeared is true only when the drafter was credited on a pick in the draft.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
  "Performance",
  "CA1812:Avoid uninstantiated internal classes",
  Justification = "Instantiated by Dapper."
)]
internal sealed class DrafterDraftRow
{
  public Guid DrafterId { get; set; }
  public string DrafterPublicId { get; set; } = string.Empty;
  public string DrafterPersonPublicId { get; set; } = string.Empty;
  public string DrafterName { get; set; } = string.Empty;
  public Guid DraftId { get; set; }
  public string DraftPublicId { get; set; } = string.Empty;
  public string DraftTitle { get; set; } = string.Empty;
  public bool Appeared { get; set; }

  /// <summary>The site's episode number for the draft (lowest across its parts). Orders streak records.</summary>
  public int? EpisodeNumber { get; set; }

  public int PicksPlayed { get; set; }
  public int PicksLanded { get; set; }
  public int PicksVetoed { get; set; }
  public int PicksSaved { get; set; }
  public int PicksRemovedByCommissioner { get; set; }
  public int No1Landed { get; set; }
  public int No1Vetoed { get; set; }

  public int VetoesUsed { get; set; }
  public int VetoesOverridden { get; set; }
  public int SelfVetoes { get; set; }
  public int No1VetoesUsed { get; set; }
  public int OverridesDeployed { get; set; }
}
