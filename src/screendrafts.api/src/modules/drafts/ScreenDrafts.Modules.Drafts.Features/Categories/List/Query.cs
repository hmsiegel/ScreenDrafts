namespace ScreenDrafts.Modules.Drafts.Features.Categories.List;

internal sealed record Query(bool IncludeDeleted) : IQuery<CategoryCollectionResponse>;

