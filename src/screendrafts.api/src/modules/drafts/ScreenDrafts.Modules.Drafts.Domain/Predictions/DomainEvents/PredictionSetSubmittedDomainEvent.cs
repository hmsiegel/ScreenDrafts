namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.DomainEvents;

public sealed class PredictionSetSubmittedDomainEvent(
  DraftPredictionSetId setId,
  ContestantId contestantId,
  DraftPartId draftPartId,
  PredictionSeasonId seasonId) : DomainEvent
{
  public DraftPredictionSetId SetId { get; init; } = setId;
  public ContestantId ContestantId { get; init; } = contestantId;
  public DraftPartId DraftPartId { get; init; } = draftPartId;
  public PredictionSeasonId SeasonId { get; init; } = seasonId;
}
