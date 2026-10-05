namespace ScreenDrafts.Modules.Drafts.Features.Predictions.AssignSurrogate;

internal sealed record AssignSurrogateRequest
{
  public string SurrogateSetPublicId { get; init; } = default!;
  public int MergePolicy { get; init; }
}
