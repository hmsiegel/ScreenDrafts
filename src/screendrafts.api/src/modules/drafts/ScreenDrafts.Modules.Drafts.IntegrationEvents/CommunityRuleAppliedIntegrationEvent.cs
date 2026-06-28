namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

public sealed class CommunityRuleAppliedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  string draftPartPublicId,
  int tmdbId,
  string? movieTitle,
  int playOrder,
  int boardPosition,
  int ruleKind,
  int targetSlot
) : IntegrationEvent(id, occurredOnUtc)
{
  public string DraftPartPublicId { get; init; } = draftPartPublicId;
  public int TmdbId { get; init; } = tmdbId;
  public string? MovieTitle { get; init; } = movieTitle;
  public int PlayOrder { get; init; } = playOrder;
  public int BoardPosition { get; init; } = boardPosition;

  /// <summary>0 = BoostersVeto, 1 = BoostersPick</summary>
  public int RuleKind { get; init; } = ruleKind;
  public int TargetSlot { get; init; } = targetSlot;
}
