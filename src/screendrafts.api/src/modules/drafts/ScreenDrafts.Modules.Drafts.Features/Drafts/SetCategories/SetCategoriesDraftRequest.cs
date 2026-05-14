namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetCategories;

internal sealed record SetCategoriesDraftRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; set; } = default!;
  public List<string> CategoryIds { get; set; } = [];
}
