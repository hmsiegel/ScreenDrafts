namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetDraftPartPredictionRules;

internal sealed record GetDraftPartPredictionRulesQuery
  : IQuery<GetDraftPartPredictionRulesResponse?>
{
  public required string DraftPartPublicId { get; init; }
}
