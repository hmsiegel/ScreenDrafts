namespace ScreenDrafts.Modules.Drafts.Features.Predictions.SearchPredictionContestants;

internal sealed record SearchPredictionContestantsQuery
  : IQuery<SearchPredictionContestantsResponse>
{
  public string? Name { get; init; }
  public int PageSize { get; init; }
}
