namespace ScreenDrafts.Modules.Drafts.Features.Categories.List;

internal sealed record ListCategoriesQuery(bool IncludeDeleted) : IQuery<CategoryCollectionResponse>;


