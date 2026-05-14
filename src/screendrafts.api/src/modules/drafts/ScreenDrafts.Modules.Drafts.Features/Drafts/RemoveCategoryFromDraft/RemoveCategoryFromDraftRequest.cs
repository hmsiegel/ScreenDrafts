namespace ScreenDrafts.Modules.Drafts.Features.Drafts.RemoveCategoryFromDraft;

internal sealed record RemoveCategoryFromDraftRequest
{
  [FromRoute(Name = "publicId")]
  public string DraftId { get; init; } = default!;

  [FromRoute(Name = "categoryId")]
  public string CategoryId { get; init; } = default!;
}
