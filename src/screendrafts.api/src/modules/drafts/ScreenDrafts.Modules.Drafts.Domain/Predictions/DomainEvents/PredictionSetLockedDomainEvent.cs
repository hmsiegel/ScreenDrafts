namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.DomainEvents;

public sealed class PredictionSetLockedDomainEvent(
  DraftPredictionSetId setId,
  ContestantId contestantId,
  DraftPartId draftPartId,
  DateTime lockedAtUtc) : DomainEvent
{
  public DraftPredictionSetId SetId { get; init; } = setId;
  public ContestantId ContestantId { get; init; } = contestantId;
  public DraftPartId DraftPartId { get; init; } = draftPartId;
  public DateTime LockedAtUtc { get; init; } = lockedAtUtc;
}
