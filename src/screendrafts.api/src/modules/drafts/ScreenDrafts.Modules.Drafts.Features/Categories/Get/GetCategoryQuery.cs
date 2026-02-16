namespace ScreenDrafts.Modules.Drafts.Features.Categories.Get;

internal sealed record GetCategoryQuery(string PublicId) : IQuery<CategoryResponse>;

