namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetDraftPartPredictions;

internal sealed record PredictionResultResponse
{
  public required int CorrectCount { get; init; }
  public required bool ShootsTheMoon { get; init; }
  public required int PointsAwarded { get; init; }
  public required DateTime ScoredAtUtc { get; init; }
}
