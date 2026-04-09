namespace ScreenDrafts.Modules.Drafts.Features.Predictions.ScoreDraftPartPredictions;

internal sealed record ScoreDraftPartPredictionsRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartPublicId { get; init; } = default!;

  public IReadOnlyList<string> FinalMediaPublicIds { get; init; } = [];
}
