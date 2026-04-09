namespace ScreenDrafts.Modules.Drafts.Features.Predictions.LockPredictionSet;

internal sealed record LockPredictionSetCommand : ICommand
{
  public string DraftPartPublicId { get; init; } = default!;
  public string SetPublicId { get; init; } = default!;
}
