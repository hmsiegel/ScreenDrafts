namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Services;

/// <summary>
/// Resolves the final points credited to a primary contestant when a surrogate
/// predicts on their behalf.
/// 
/// Caller flow
/// 1. Score the primary set (may have 0 entries if the contestant was absent)
/// 2. Score the surrogate set
/// 3. Call <see cref="Resolve(SurrogateAssignment, PredictionResult?, PredictionResult)"/> to get the definitive points.
/// 4. Call <see cref="PredictionStanding.Add(decimal, int, decimal, DateTime)"/> with the resolved amount.
/// </summary>
public sealed class SurrogateScoreResolver
{
  private SurrogateScoreResolver() { }

  /// <summary>
  /// Calculates the resolved score for a surrogate assignment based on the specified merge policy and prediction
  /// results.
  /// </summary>
  /// <remarks>The resolved score is determined by the merge policy: either the higher of the two scores or the
  /// sum of both. If the primary result is null, its score is treated as zero.</remarks>
  /// <param name="assignment">The surrogate assignment that defines the merge policy to use when combining prediction results. Cannot be null.</param>
  /// <param name="primaryResult">The primary prediction result to consider in the merge. May be null; if null, its score is treated as zero.</param>
  /// <param name="surrogateResult">The surrogate prediction result to consider in the merge. Cannot be null.</param>
  /// <returns>An integer representing the resolved score, calculated according to the assignment's merge policy.</returns>
  /// <exception cref="InvalidOperationException">Thrown if the merge policy specified in assignment is not recognized.</exception>
  public static int Resolve(
    SurrogateAssignment assignment,
    PredictionResult? primaryResult,
    PredictionResult surrogateResult)
  {
    ArgumentNullException.ThrowIfNull(assignment);
    ArgumentNullException.ThrowIfNull(surrogateResult);

    var primaryPoints = primaryResult?.PointsAwarded ?? 0;
    var surrogatePoints = surrogateResult.PointsAwarded;

    return assignment.MergePolicy.Name switch
    {
      nameof(MergePolicy.UseHigherScore) => Math.Max(primaryPoints, surrogatePoints),
      nameof(MergePolicy.UseBothScores) => primaryPoints + surrogatePoints,
      _ => throw new InvalidOperationException($"Unknown merge policy: {assignment.MergePolicy}")
    };
  }
}
