using ScreenDrafts.Modules.Drafts.Features.Predictions.Common;

namespace ScreenDrafts.Modules.Drafts.Features.Predictions.ListPredictionSeasons;

internal sealed record ListPredictionSeasonsResult
{
  public required IReadOnlyList<PredictionSeasonSummaryResponse> Seasons { get; init; }
}
