namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetDraftPartPredictions;

internal sealed record GetDraftPartPredictionsQuery : IQuery<IReadOnlyList<DraftPartPredictionResponse>>
{
  public required string DraftPartPublicId { get; init; }
}
