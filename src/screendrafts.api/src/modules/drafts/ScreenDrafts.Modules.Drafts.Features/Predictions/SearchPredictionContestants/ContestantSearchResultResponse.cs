namespace ScreenDrafts.Modules.Drafts.Features.Predictions.SearchPredictionContestants;

internal sealed record ContestantSearchResultResponse
{
  public required string PublicId { get; init; }
  public required string DisplayName { get; init; }
}
