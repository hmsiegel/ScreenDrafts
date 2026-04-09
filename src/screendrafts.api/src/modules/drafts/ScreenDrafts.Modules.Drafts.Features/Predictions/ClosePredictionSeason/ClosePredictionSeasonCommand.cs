namespace ScreenDrafts.Modules.Drafts.Features.Predictions.ClosePredictionSeason;

internal sealed record ClosePredictionSeasonCommand : ICommand
{
  public string SeasonPublicId { get; init; } = default!;
  public DateOnly EndsOn { get; init; }
}
