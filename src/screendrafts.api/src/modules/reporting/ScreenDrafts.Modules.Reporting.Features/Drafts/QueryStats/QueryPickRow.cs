namespace ScreenDrafts.Modules.Reporting.Features.Drafts.QueryStats;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Dapper.")]
internal sealed class QueryPickRow
{
  public Guid PickId { get; set; }
  public Guid DraftId { get; set; }
  public string DraftPublicId { get; set; } = string.Empty;
  public string DraftTitle { get; set; } = string.Empty;
  public string DraftType { get; set; } = string.Empty;
  public string SeriesName { get; set; } = string.Empty;

  /// <summary>The site's episode number for the draft. Null for a draft with no main-feed release.</summary>
  public int? EpisodeNumber { get; set; }

  public string MediaPublicId { get; set; } = string.Empty;
  public string MediaTitle { get; set; } = string.Empty;

  /// <summary>Not removed by the commissioner and not vetoed with the veto standing.</summary>
  public bool Landed { get; set; }

  /// <summary>Vetoed with the veto standing.</summary>
  public bool VetoStanding { get; set; }

  public bool CommissionerOverridden { get; set; }
}
