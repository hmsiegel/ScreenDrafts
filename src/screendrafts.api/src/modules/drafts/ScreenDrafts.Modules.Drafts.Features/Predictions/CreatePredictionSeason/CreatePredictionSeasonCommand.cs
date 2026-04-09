namespace ScreenDrafts.Modules.Drafts.Features.Predictions.CreatePredictionSeason;

internal sealed record CreatePredictionSeasonCommand : ICommand<string>
{
  public required int Number { get; init; }
  public required DateOnly StartsOn { get; init; }
}
