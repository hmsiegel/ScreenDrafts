namespace ScreenDrafts.Modules.Drafts.Application.Hosts.Queries.GetHost;

public sealed record GetHostQuery(Guid HostId) : IQuery<HostResponse>;
