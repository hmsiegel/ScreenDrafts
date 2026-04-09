namespace ScreenDrafts.Modules.Drafts.Features.Predictions.ClosePredictionSeason;

internal sealed record ClosePredictionSeasonRequest
{
  [FromRoute(Name = "seasonId")]
  public string SeasonPublicId { get; init; } = default!;

  public DateOnly EndsOn { get; init; }
}
