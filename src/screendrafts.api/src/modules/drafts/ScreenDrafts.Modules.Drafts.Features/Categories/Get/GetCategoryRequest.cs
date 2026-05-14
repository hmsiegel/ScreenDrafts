namespace ScreenDrafts.Modules.Drafts.Features.Categories.Get;

internal sealed record GetCategoryRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;
}

