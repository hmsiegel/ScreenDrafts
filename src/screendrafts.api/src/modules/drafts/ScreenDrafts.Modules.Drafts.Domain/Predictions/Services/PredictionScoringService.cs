namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Services;

/// <summary>
/// Domain service that scores a <see cref="DraftPredictionSet"/> against the
/// finalized picks of a <see cref="DraftPart"/> according to the rules defined in <see cref="DraftPartPredictionRule"/>.
///
/// Scoring rules
///  - 1 point per correct prediction
///  - "Shoot the moon": predicting every required title correctly earns double points,
///    but only for contestants eligible for the bonus (see isShootTheMoonEligible param —
///    eligibility is a caller concern, not something this service looks up itself)
///  - Ordered* modes require an exact position match: the entry's OrderIndex (1-based)
///    must equal the actual 1-based slot that TmdbId landed at in finalTmdbIds
///  - Unordered* modes only require the predicted title to appear anywhere in the
///    scored pool (all final picks, or the top N if TopN is set)
///
/// Inputs:
///  - The set to score (must be locked)
///  - The ordered list of media public Ids that made the final list
///  - The prediction rules to apply
///  - Whether this contestant is eligible for the Shoot the Moon bonus
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
    IReadOnlyList<int> finalTmdbIds,
    DraftPartPredictionRule rules,
    bool isShootTheMoonEligible,
    DateTime scoredAtUtc
  )
  {
    ArgumentNullException.ThrowIfNull(set);
    ArgumentNullException.ThrowIfNull(finalTmdbIds);
    ArgumentNullException.ThrowIfNull(rules);

    if (!set.IsLocked)
    {
      return Result.Failure<PredictionResult>(PredictionErrors.SetAlreadyLocked);
    }

    var isOrderedMode =
      rules.PredictionMode == PredictionMode.OrderedAll
      || rules.PredictionMode == PredictionMode.OrderedTopN;

    // Pool size = how many final slots count toward scoring. TopN modes only
    // count picks landing at position <= TopN; *All modes count every final slot.
    var poolSize = rules.TopN ?? finalTmdbIds.Count;

    bool IsEntryCorrect(PredictionEntry entry)
    {
      if (isOrderedMode)
      {
        // Exact position match: the entry's predicted rank must equal the
        // actual final-board slot holding that title, and that slot must
        // fall within the scored pool (relevant for OrderedTopN).
        return entry.OrderIndex.HasValue
          && entry.OrderIndex.Value >= 1
          && entry.OrderIndex.Value <= poolSize
          && entry.OrderIndex.Value <= finalTmdbIds.Count
          && finalTmdbIds[entry.OrderIndex.Value - 1] == entry.TmdbId;
      }

      // Unordered: membership only — predicted anywhere in the scored pool.
      var scoringPool = rules.TopN.HasValue
        ? finalTmdbIds.Take(rules.TopN.Value).ToHashSet()
        : [.. finalTmdbIds];

      return scoringPool.Contains(entry.TmdbId);
    }

    var correctCount = set.Entries.Count(IsEntryCorrect);

    // Shoot the Moon requires both a perfect predicted set AND bonus eligibility —
    // some contestants are permanently excluded from the bonus regardless of score.
    var shootTheMoon = isShootTheMoonEligible && correctCount == rules.RequiredCount;

    var totalPoints = shootTheMoon ? correctCount * 2 : correctCount;

    foreach (var entry in set.Entries)
    {
      entry.MarkCorrect(IsEntryCorrect(entry));
    }

    return PredictionResult.Create(
      predictionSet: set,
      correctCount: correctCount,
      shootTheMoon: shootTheMoon,
      pointsAwarded: totalPoints,
      scoredAtUtc: scoredAtUtc
    );
  }
}
