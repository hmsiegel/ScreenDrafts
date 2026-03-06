namespace ScreenDrafts.Modules.Drafts.Features.Drafts.RemoveCategoryFromDraft;

internal sealed record RemoveCategoryFromDraftRequest
{
  [FromRoute(Name = "publicId")]
  public required string DraftId { get; init; }

  [FromRoute(Name = "categoryId")]
  public required string CategoryId { get; init; }
}
