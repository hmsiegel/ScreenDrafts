namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetRecordBook;

/// <summary>
/// One drafter at one board position of one draft part, where at least one of their picks was vetoed.
/// A vetoed pick is replaced at the same position, so TimesVetoed is the length of the veto chain.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
  "Performance",
  "CA1812:Avoid uninstantiated internal classes",
  Justification = "Instantiated by Dapper."
)]
internal sealed class PickSlotRow
{
  public Guid DrafterId { get; set; }
  public string DrafterPublicId { get; set; } = string.Empty;
  public string DrafterPersonPublicId { get; set; } = string.Empty;
  public string DrafterName { get; set; } = string.Empty;
  public Guid DraftId { get; set; }
  public string DraftPublicId { get; set; } = string.Empty;
  public string DraftTitle { get; set; } = string.Empty;
  public string PartPublicId { get; set; } = string.Empty;
  public int? SubDraftIndex { get; set; }
  public int Position { get; set; }

  /// <summary>Picks at this position vetoed with the veto standing.</summary>
  public int TimesVetoed { get; set; }
}
