namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Services;

/// <summary>
/// Domain service that scores a <see cref="DraftPredictionSet"/> against the 
/// finalized picks of a <see cref="DraftPart"/> according to the rules defined in <see cref="DraftPartPredictionRule"/>.
/// 
/// Scoring rules
///  - 1 point per correct prediction
///  - "Shoot the moon": predicting every required title correctly earns double points
/// 
/// Inputs:
///  - The set to score (must be locked)
///  - The ordered list of media public Ids that made the final list
///  - The prediction rules to apply
///  - The UTC time scoring is being performed
///  
/// The caller (application-layer handler) is responsibles for:
/// - Persisting the returned <see cref="PredictionResult"/>.
/// - Passing the result to <see cref="SurrogateScoreResolver"/> when a surrogate assignment exists.
/// - Calling <see cref="PredictionStanding.Add(decimal, int, decimal, DateTime)"/> with the resolved points.
/// </summary>
public sealed class PredictionScoringService
{
  private PredictionScoringService() { }

  /// <summary>
  /// Ordered final picks. Index 0 = position 1
  /// For unordered modes, pass them in any consistent order
  /// </summary>
  public static Result<PredictionResult> Score(
    DraftPredictionSet set,
    IReadOnlyList<string> finalMediaPublicIds,
    DraftPartPredictionRule rules,
    DateTime scoredAtUtc)
  {
    ArgumentNullException.ThrowIfNull(set);
    ArgumentNullException.ThrowIfNull(finalMediaPublicIds);
    ArgumentNullException.ThrowIfNull(rules);

    if (!set.IsLocked)
    {
      return Result.Failure<PredictionResult>(PredictionErrors.SetAlreadyLocked);
    }

    var scoringPool = rules.TopN.HasValue
      ? [.. finalMediaPublicIds.Take(rules.TopN.Value)]
      : finalMediaPublicIds.ToHashSet();


    var predictedIds = set.Entries
      .Select(e => e.MediaPublicId)
      .ToList();

    var correctCount = predictedIds.Count(predictedId => scoringPool.Contains(predictedId));

    var shootTheMoon = correctCount == rules.RequiredCount;

    var totalPoints = shootTheMoon ? correctCount * 2 : correctCount;

    return PredictionResult.Create(
      predictionSet: set,
      correctCount: correctCount,
      shootTheMoon: shootTheMoon,
      pointsAwarded: totalPoints,
      scoredAtUtc: scoredAtUtc);
  }
}
