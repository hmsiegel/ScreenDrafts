namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class CommunityRuleVetoAppliedDomainEvent(
  Guid draftPartId,
  string draftPartPublicId,
  Guid draftId,
  string draftPublicId,
  int tmdbId,
  string? movieTitle,
  int playOrder,
  int boardPosition,
  int ruleKind,
  int targetSlot
) : DomainEvent
{
  public Guid DraftPartId { get; init; } = draftPartId;
  public string DraftPartPublicId { get; init; } = draftPartPublicId;
  public Guid DraftId { get; init; } = draftId;
  public string DraftPublicId { get; init; } = draftPublicId;
  public int TmdbId { get; init; } = tmdbId;
  public string? MovieTitle { get; init; } = movieTitle;
  public int PlayOrder { get; init; } = playOrder;
  public int BoardPosition { get; init; } = boardPosition;

  /// <summary>0 = BoostersVeto, 1 = BoostersPick</summary>
  public int RuleKind { get; init; } = ruleKind;
  public int TargetSlot { get; init; } = targetSlot;
}
