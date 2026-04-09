namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Entities;

/// <summary>
/// The scored result for one <see cref="DraftPredictionSet"/> after a draft part's
/// picks are finalized. Created by <see cref="PredictionScoringService"/> and persisted
/// so the standing update is idempotent and auditable.
/// </summary>
public sealed class PredictionResult : Entity<PredictionResultId>
{
  private PredictionResult(
    DraftPredictionSet predictionSet,
    int correctCount,
    bool shootTheMoon,
    int pointsAwarded,
    DateTime scoredAtUtc,
    PredictionResultId? id = null)
    : base(id ?? PredictionResultId.CreateUnique())
  {
    PredictionSet = predictionSet;
    SetId = predictionSet.Id;
    CorrectCount = correctCount;
    ShootTheMoon = shootTheMoon;
    PointsAwarded = pointsAwarded;
    ScoredAtUtc = scoredAtUtc;
  }

  private PredictionResult()
  {
  }

  public DraftPredictionSet PredictionSet { get; private set; } = default!;
  public DraftPredictionSetId SetId { get; private set; } = default!;

  /// <summary>
  /// Number of predicted titles that appearerd on the final list.
  /// </summary>
  public int CorrectCount { get; private set; }

  /// <summary>
  /// True when the contestant predicted every required title correctly,
  /// earning double points for the draft part
  /// </summary>
  public bool ShootTheMoon { get; private set; }

  /// <summary>
  /// Total points awarded, including any bonus
  /// </summary>
  public int PointsAwarded { get; private set; }

  public DateTime ScoredAtUtc { get; private set; }

  public static PredictionResult Create(
    DraftPredictionSet predictionSet,
    int correctCount,
    bool shootTheMoon,
    int pointsAwarded,
    DateTime scoredAtUtc)
  {
    ArgumentNullException.ThrowIfNull(predictionSet);
    Guard.Against.Negative(correctCount);
    Guard.Against.Negative(pointsAwarded);

    return new PredictionResult(
      predictionSet: predictionSet,
      correctCount: correctCount,
      shootTheMoon: shootTheMoon,
      pointsAwarded: pointsAwarded,
      scoredAtUtc: scoredAtUtc);
  }
}
