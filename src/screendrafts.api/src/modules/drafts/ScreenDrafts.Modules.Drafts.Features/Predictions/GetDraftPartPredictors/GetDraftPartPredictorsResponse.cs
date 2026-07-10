namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetDraftPartPredictors;

internal sealed record GetDraftPartPredictorsResponse
{
  public required IReadOnlyList<PredictorResponse> Predictors { get; init; }
}
