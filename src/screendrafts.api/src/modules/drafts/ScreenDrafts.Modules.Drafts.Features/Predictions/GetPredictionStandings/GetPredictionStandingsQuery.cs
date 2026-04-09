namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetPredictionStandings;

internal sealed record GetPredictionStandingsQuery : IQuery<PredictionStandingsResponse>
{
  public string SeasonPublicId { get; init; } = default!;
}
