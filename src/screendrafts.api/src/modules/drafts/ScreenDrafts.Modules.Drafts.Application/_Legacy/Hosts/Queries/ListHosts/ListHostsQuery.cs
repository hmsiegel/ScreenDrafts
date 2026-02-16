using ScreenDrafts.Modules.Drafts.Application._Legacy.Hosts.Queries.GetHost;

namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Hosts.Queries.ListHosts;

public sealed record ListHostsQuery(
  int Page,
  int PageSize,
  string? Search = null,
  string? Sort = null,
  string? Dir = null) : IQuery<PagedResult<HostResponse>>;
