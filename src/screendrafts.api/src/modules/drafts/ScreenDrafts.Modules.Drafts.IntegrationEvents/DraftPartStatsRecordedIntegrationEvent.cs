namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

/// <summary>
/// Full pick/veto/credit facts for a completed draft part, feeding Reporting's Record Book.
/// Unlike DraftPartCompletedIntegrationEvent, this includes vetoed and commissioner-overridden
/// picks and names every participant. Reporting replaces the part's facts on every receipt.
/// </summary>
public sealed class DraftPartStatsRecordedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid draftId,
  string draftPublicId,
  string draftPartPublicId,
  int partIndex,
  string draftTitle,
  string draftType,
  string seriesName,
  int canonicalPolicyValue,
  IReadOnlyList<StatsPickRecord> picks,
  bool hasMainFeedRelease
) : IntegrationEvent(id, occurredOnUtc)
{
  public Guid DraftId { get; set; } = draftId;
  public string DraftPublicId { get; set; } = draftPublicId;
  public string DraftPartPublicId { get; set; } = draftPartPublicId;
  public int PartIndex { get; set; } = partIndex;
  public string DraftTitle { get; set; } = draftTitle;
  public string DraftType { get; set; } = draftType;
  public string SeriesName { get; set; } = seriesName;
  public int CanonicalPolicyValue { get; set; } = canonicalPolicyValue;
  public bool HasMainFeedRelease { get; set; } = hasMainFeedRelease;
  public IReadOnlyList<StatsPickRecord> Picks { get; set; } = picks;
}
