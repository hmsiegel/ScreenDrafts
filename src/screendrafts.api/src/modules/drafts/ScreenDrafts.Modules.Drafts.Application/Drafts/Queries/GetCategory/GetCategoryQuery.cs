namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetCategory;

public sealed record GetCategoryQuery(Guid Id) : IQuery<CategoryResponse?>;
