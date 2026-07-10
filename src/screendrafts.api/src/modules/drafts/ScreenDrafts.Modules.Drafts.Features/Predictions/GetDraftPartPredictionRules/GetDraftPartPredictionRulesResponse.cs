namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetDraftPartPredictionRules;

internal sealed record GetDraftPartPredictionRulesResponse
{
  public required int PredictionMode { get; init; }
  public required int RequiredCount { get; init; }
  public int? TopN { get; init; }
  public DateTime? DeadlineUtc { get; init; }
}
