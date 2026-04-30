namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetPredictionStandings;

// ============================================================
// Request
// ============================================================

internal sealed record GetPredictionStandingsRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;
}
