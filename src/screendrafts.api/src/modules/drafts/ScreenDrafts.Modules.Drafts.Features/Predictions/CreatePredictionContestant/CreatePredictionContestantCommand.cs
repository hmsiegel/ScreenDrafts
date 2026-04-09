namespace ScreenDrafts.Modules.Drafts.Features.Predictions.CreatePredictionContestant;

internal sealed record CreatePredictionContestantCommand : ICommand<string>
{
  public required string PersonPublicId { get; init; }
}
