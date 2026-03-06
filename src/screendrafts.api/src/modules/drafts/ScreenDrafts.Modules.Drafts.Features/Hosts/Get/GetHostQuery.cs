namespace ScreenDrafts.Modules.Drafts.Features.Hosts.Get;

internal sealed record GetHostQuery : IQuery<GetHostResponse>
{
  public required string HostPublicId { get; init; }
}

