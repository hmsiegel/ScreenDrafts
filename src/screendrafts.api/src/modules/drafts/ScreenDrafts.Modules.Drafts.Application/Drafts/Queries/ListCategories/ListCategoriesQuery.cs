
namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.ListCategories;

public sealed record ListCategoriesQuery(
  int Page,
  int PageSize,
  string? Q = null) : IQuery<PagedResult<CategoryResponse?>>;
