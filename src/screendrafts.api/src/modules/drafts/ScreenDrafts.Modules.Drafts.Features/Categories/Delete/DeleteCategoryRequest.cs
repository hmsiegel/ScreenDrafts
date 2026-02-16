namespace ScreenDrafts.Modules.Drafts.Features.Categories.Delete;

internal sealed record DeleteCategoryRequest
{
  public required string PublicId { get; init; }
}

