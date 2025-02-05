namespace ScreenDrafts.Modules.Drafts.Application.Hosts.Queries.ListHosts;
public sealed record ListHostsQuery() : IQuery<IReadOnlyCollection<HostResponse>>;
