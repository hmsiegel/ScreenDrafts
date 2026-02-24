namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraftPart;

internal sealed record CreateDraftPartRequest
{
  [FromRoute(Name = "draftId")]
  public required string DraftId { get; init; }
  public int PartIndex { get; init; } 
  public int MinimumPosition { get; init; }
  public int MaximumPosition { get; init; }
}
