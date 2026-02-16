namespace ScreenDrafts.Modules.Drafts.Features.Categories.Create;

internal sealed record CreateCategoryRequest
{
  public required string Name { get; init; }
  public required string Description { get; init; }
}



