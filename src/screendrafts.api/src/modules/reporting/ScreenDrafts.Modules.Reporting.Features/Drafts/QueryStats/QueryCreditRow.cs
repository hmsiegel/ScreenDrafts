namespace ScreenDrafts.Modules.Reporting.Features.Drafts.QueryStats;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
  "Performance",
  "CA1812:Avoid uninstantiated internal classes",
  Justification = "Instantiated by Dapper."
)]
internal sealed class QueryCreditRow
{
  public Guid PickId { get; set; }
  public Guid DrafterId { get; set; }
  public string DrafterPersonPublicId { get; set; } = string.Empty;
  public string DrafterPublicId { get; set; } = string.Empty;
  public string DrafterName { get; set; } = string.Empty;
}
