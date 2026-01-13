namespace ScreenDrafts.Modules.Drafts.Features.Categories.Get;

internal sealed record Query(string PublicId) : IQuery<CategoryResponse>;
