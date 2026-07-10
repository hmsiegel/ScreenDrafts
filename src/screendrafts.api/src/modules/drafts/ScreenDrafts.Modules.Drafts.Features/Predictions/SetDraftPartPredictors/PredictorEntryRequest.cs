namespace ScreenDrafts.Modules.Drafts.Features.Predictions.SetDraftPartPredictors;

internal sealed record PredictorEntryRequest
{
  public required string ContestantPublicId { get; init; }
  public string? AllowedSubmitterPersonPublicId { get; init; }
}
