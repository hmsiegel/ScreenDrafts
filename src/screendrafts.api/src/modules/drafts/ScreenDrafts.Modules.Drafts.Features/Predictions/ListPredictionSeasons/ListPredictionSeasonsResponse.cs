namespace ScreenDrafts.Modules.Drafts.Features.Predictions.ListPredictionSeasons;

internal sealed record ListPredictionSeasonsResponse
{
  public IReadOnlyList<PredictionSeasonListItemResponse> Seasons { get; init; } = [];
}
