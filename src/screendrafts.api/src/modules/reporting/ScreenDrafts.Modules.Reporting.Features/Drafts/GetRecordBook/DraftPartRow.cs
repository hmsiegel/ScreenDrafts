namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetRecordBook;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Dapper.")]
internal sealed class DraftPartRow
{
  public Guid DraftId { get; set; }
  public string DraftPublicId { get; set; } = string.Empty;
  public string DraftTitle { get; set; } = string.Empty;
  public string DraftType { get; set; } = string.Empty;
  public string PartPublicId { get; set; } = string.Empty;
  public int PartIndex { get; set; }
  public int PicksPlayed { get; set; }
  public int PicksLanded { get; set; }
  public int UniqueTitlesPlayed { get; set; }
  public int PicksVetoed { get; set; }
  public int No1Vetoed { get; set; }
  public int CommissionerOverrides { get; set; }
  public int VetoesIssued { get; set; }
  public int VetoesOverridden { get; set; }
}
