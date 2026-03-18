namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraftPart;

internal sealed record CreateDraftPartRequest
{
  [FromRoute(Name = "draftId")]
  public string DraftId { get; init; } = default!;
  public int PartIndex { get; init; } 
  public int MinimumPosition { get; init; }
  public int MaximumPosition { get; init; }
}
