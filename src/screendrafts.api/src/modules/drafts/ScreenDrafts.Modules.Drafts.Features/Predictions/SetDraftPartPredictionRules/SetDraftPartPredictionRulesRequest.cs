namespace ScreenDrafts.Modules.Drafts.Features.Predictions.SetDraftPartPredictionRules;

internal sealed record SetDraftPartPredictionRulesRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartPublicId { get; init; } = default!;

  public int PredictionMode { get; init; }
  public int RequiredCount { get; init; } = 7;
  public int? TopN { get; init; }
  public DateTime? DeadlineUtc { get; init; }
}
