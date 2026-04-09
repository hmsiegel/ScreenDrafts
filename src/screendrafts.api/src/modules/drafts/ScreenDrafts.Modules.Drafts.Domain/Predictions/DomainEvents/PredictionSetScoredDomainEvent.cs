namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.DomainEvents;

public sealed class PredictionSetScoredDomainEvent(
  DraftPredictionSetId setId,
  ContestantId contestantId,
  PredictionSeasonId seasonId,
  int pointsAwarded,
  bool shootTheMoon) : DomainEvent
{
  public DraftPredictionSetId SetId { get; init; } = setId;
  public ContestantId ContestantId { get; init; } = contestantId;
  public PredictionSeasonId SeasonId { get; init; } = seasonId;
  public int PointsAwarded { get; init; } = pointsAwarded;
  public bool ShootTheMoon { get; init; } = shootTheMoon;
}
