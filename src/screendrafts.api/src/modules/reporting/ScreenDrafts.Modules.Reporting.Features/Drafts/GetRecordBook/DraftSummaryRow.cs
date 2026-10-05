namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetRecordBook;

/// <summary>One draft's numbers, summed over its parts.</summary>
internal sealed class DraftSummaryRow
{
  public Guid DraftId { get; init; }
  public string DraftPublicId { get; init; } = string.Empty;
  public string DraftTitle { get; init; } = string.Empty;
  public string DraftType { get; init; } = string.Empty;
  public int PicksLanded { get; init; }

  /// <summary>Summed over parts. A title drafted in two parts of one draft would count twice.</summary>
  public int UniqueTitlesPlayed { get; init; }

  public int PicksVetoed { get; init; }
  public int No1Vetoed { get; init; }
  public int CommissionerOverrides { get; init; }
  public int VetoesOverridden { get; init; }
}
