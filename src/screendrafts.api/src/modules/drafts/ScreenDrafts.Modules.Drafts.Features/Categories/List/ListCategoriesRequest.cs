namespace ScreenDrafts.Modules.Drafts.Features.Categories.List;

internal sealed record ListCategoriesRequest
{
  public bool IncludeDeleted { get; init; }
}

