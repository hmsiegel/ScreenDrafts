namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetDraftPartPredictions;

internal sealed record SurrogateAssignmentResponse
{
  public required string SurrogateSetPublicId { get; init; }
  public required string SurrogateContestantDisplayName { get; init; }
  public required string MergePolicy { get; init; }
}
