namespace ScreenDrafts.Modules.Drafts.Features.Predictions.ScoreDraftPartPredictions;

internal sealed record ScoreDraftPartPredictionsCommand : ICommand
{
  public required string DraftPartId { get; init; }
  public required IReadOnlyList<int> FinalTmdbIds { get; init; }
}
