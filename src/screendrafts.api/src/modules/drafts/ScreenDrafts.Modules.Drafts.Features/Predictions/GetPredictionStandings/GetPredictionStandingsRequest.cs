namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetPredictionStandings;

// ============================================================
// Request
// ============================================================

internal sealed record GetPredictionStandingsRequest
{
  [FromRoute(Name = "seasonId")]
  public string SeasonPublicId { get; init; } = default!;
}
