namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraft;

internal sealed record GetDraftRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; set; } = default!;
}
