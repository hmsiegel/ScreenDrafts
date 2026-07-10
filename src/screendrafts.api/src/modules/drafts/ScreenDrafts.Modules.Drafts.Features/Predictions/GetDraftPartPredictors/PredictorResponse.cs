namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetDraftPartPredictors;

internal sealed record PredictorResponse
{
  public required string ContestantPublicId { get; init; }
  public required string ContestantDisplayName { get; init; }
  public string? AllowedSubmitterPersonPublicId { get; init; }
  public string? AllowedSubmitterDisplayName { get; init; }
}
