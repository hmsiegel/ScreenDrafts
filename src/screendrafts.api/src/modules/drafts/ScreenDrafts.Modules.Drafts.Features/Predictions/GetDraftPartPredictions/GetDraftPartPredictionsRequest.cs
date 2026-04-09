namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetDraftPartPredictions;

internal sealed record GetDraftPartPredictionsRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartPublicId { get; init; } = default!;
}
