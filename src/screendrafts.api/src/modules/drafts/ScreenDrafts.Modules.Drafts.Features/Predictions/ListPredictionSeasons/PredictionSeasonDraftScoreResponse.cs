namespace ScreenDrafts.Modules.Drafts.Features.Predictions.ListPredictionSeasons;

internal sealed record PredictionSeasonDraftScoreResponse
{
  public string ContestantDisplayName { get; init; } = default!;
  public int PointsAwarded { get; init; }
}
