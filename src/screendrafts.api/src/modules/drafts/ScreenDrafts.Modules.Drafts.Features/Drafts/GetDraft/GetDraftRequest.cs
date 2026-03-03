namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraft;

internal sealed record GetDraftRequest
{
  [FromRoute(Name = "publicId")]
  public required string DraftId { get; set; }
}
