namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetPredictionStandings;

internal sealed record PredictionStandingsResponse
{
  public string SeasonPublicId { get; init; } = default!;
  public int SeasonNumber { get; init; }
  public int TargetPoints { get; init; }
  public bool IsClosed { get; init; }
  public IReadOnlyList<ContestantStandingResponse> Standings { get; init; } = default!;
}
