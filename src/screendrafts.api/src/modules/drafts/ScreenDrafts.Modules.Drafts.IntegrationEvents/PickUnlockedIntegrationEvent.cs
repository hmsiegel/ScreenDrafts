namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

// <summary>
/// Published when a pick that may have previously been canonical (i.e. may have fired
/// PickLockedIntegrationEvent) is struck — either by an ordinary veto, or by a commissioner
/// override (which, for honorific purposes, behaves identically to a veto: the pick is removed
/// from the board and the participant gets to pick again).
///
/// Deliberately fired unconditionally by VetoAppliedDomainEventHandler and
/// CommissionerOverrideAppliedDomainEventHandler — neither re-checks CanonicalPolicyValue /
/// HasMainFeedRelease before publishing. Reporting's RevertMovieHonorificCommandHandler absorbs
/// the "this pick was never canonical to begin with" case as a harmless no-op DELETE, so the two
/// gates (lock-side and unlock-side) never need to be kept in sync by hand.
/// </summary>
public sealed class PickUnlockedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid draftPartId,
  string draftPartPublicId,
  Guid draftId,
  string draftPublicId,
  string moviePublicId,
  string movieTitle,
  int tmdbId,
  int boardPosition,
  Guid playedByParticipantId,
  int playedByParticipantKind,
  PickUnlockReason unlockReason
) : IntegrationEvent(id, occurredOnUtc)
{
  public Guid DraftPartId { get; init; } = draftPartId;
  public string DraftPartPublicId { get; init; } = draftPartPublicId;
  public Guid DraftId { get; init; } = draftId;
  public string DraftPublicId { get; init; } = draftPublicId;
  public string MoviePublicId { get; init; } = moviePublicId;
  public string MovieTitle { get; init; } = movieTitle;
  public int TmdbId { get; init; } = tmdbId;
  public int BoardPosition { get; init; } = boardPosition;
  public Guid PlayedByParticipantId { get; init; } = playedByParticipantId;
  public int PlayedByParticipantKind { get; init; } = playedByParticipantKind;
  public PickUnlockReason UnlockReason { get; init; } = unlockReason;
}

/// <summary>
/// Distinguishes why a pick was unlocked, for downstream consumers (live announcement banner,
/// eventual completion-summary screen) that want different copy for the two cases even though
/// the honorific-revert mechanics are identical for both.
/// </summary>
public enum PickUnlockReason
{
  Vetoed = 0,
  CommissionerOverride = 1,
}
