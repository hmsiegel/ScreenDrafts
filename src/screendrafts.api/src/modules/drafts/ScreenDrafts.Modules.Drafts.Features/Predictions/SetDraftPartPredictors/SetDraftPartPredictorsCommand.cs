namespace ScreenDrafts.Modules.Drafts.Features.Predictions.SetDraftPartPredictors;

internal sealed record SetDraftPartPredictorsCommand : ICommand
{
  public required string DraftPartPublicId { get; init; }
  public required IReadOnlyList<PredictorEntryDto> Predictors { get; init; }
}
