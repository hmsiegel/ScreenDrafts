namespace ScreenDrafts.Modules.Drafts.Features.Predictions.ScoreDraftPartPredictions;

internal sealed record ScoreDraftPartPredictionsCommand : ICommand
{
  public required string DraftPartPublicId { get; init; }
  public required IReadOnlyList<string> FinalMediaPublicIds { get; init; }
}
