namespace ScreenDrafts.Modules.Drafts.Features.Predictions.SetDraftPartPredictionRules;

internal sealed record SetDraftPartPredictionRulesCommand : ICommand
{
  public string DraftPartPublicId { get; init; } = default!;
  public required int PredictionMode { get; init; }
  public required int RequiredCount { get; init; } = 7;
  public int? TopN { get; init; }
  public DateTime? DeadlineUtc { get; init; }
}
