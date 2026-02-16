namespace ScreenDrafts.Modules.Drafts.Features.Categories.Get;

internal sealed record GetCategoryRequest
{
  [FromRoute(Name = "publicId")]
  public required string PublicId { get; init; }
}

