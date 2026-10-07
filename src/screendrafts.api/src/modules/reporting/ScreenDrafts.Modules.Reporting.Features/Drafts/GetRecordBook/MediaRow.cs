namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetRecordBook;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by Dapper.")]
internal sealed class MediaRow
{
  public string MediaPublicId { get; set; } = string.Empty;
  public string MediaTitle { get; set; } = string.Empty;
  public int TimesDrafted { get; set; }
  public int TimesDraftedNo1 { get; set; }
}
