namespace ScreenDrafts.Modules.Drafts.Features.Predictions.SetDraftPartPredictors;

internal sealed record SetDraftPartPredictorsRequest
{
  public string DraftPartId { get; init; } = default!;
  public required IReadOnlyList<PredictorEntryRequest> Predictors { get; init; }
}
