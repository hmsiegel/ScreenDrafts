namespace ScreenDrafts.Modules.Drafts.Features.Categories.Restore;

internal sealed record RestoreCategoryRequest
{
  [FromRoute(Name = "publicId")]
  public required string PublicId { get; init; }
}

