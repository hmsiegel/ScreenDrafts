namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetCategories;

internal sealed record SetCategoriesDraftRequest
{
  [FromRoute(Name = "publicId")]
  public string DraftId { get; set; } = default!;
  public List<string> CategoryIds { get; set; } = [];
}
