namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Hosts.Queries.GetHost;

public sealed record GetHostQuery(Guid HostId) : IQuery<HostResponse>;
