namespace ScreenDrafts.Modules.Drafts.Application.Hosts.Queries.ListHosts;

public sealed record ListHostsQuery(
  int Page,
  int PageSize,
  string? Search = null,
  string? Sort = null,
  string? Dir = null) : IQuery<PagedResult<HostResponse>>;
