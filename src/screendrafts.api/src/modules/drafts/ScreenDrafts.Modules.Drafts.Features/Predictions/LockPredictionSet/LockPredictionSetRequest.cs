namespace ScreenDrafts.Modules.Drafts.Features.Predictions.LockPredictionSet;

internal sealed record LockPredictionSetRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartPublicId { get; init; } = default!;

  [FromRoute(Name = "setId")]
  public string SetPublicId { get; init; } = default!;
}
