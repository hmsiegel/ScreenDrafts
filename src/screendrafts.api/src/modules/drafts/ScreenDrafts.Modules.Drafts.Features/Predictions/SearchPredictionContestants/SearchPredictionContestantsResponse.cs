namespace ScreenDrafts.Modules.Drafts.Features.Predictions.SearchPredictionContestants;

internal sealed record SearchPredictionContestantsResponse
{
  public required IReadOnlyList<ContestantSearchResultResponse> Items { get; init; }
}
