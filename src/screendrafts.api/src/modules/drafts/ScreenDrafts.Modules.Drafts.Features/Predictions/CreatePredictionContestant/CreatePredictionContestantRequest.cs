namespace ScreenDrafts.Modules.Drafts.Features.Predictions.CreatePredictionContestant;

internal sealed record CreatePredictionContestantRequest
{
  public string PersonPublicId { get; init; } = default!;
}
