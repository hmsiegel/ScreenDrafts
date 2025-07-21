namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.ListDrafters;

public sealed record ListDraftersQuery(
  int Page,
  int PageSize,
  string? Search = null,
  string? Sort = null,
  string? Dir = null) : IQuery<PagedResult<DrafterResponse>>;

