namespace ScreenDrafts.Modules.Drafts.Features.Predictions.AssignSurrogate;

internal sealed record AssignSurrogateRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartPublicId { get; init; } = default!;

  [FromRoute(Name = "setId")]
  public string PrimarySetPublicId { get; init; } = default!;

  public string SurrogateSetPublicId { get; init; } = default!;
  public int MergePolicy { get; init; }
}
