namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetDraftPartPredictors;

internal sealed record GetDraftPartPredictorsQuery : IQuery<GetDraftPartPredictorsResponse>
{
  public required string DraftPartPublicId { get; init; }
}
