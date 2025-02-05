namespace ScreenDrafts.Modules.Drafts.Application.Hosts.Queries.GetHost;

public sealed record HostResponse(Guid Id, string Name)
{
  public HostResponse()
      : this(Guid.Empty, string.Empty)
  {
  }
}
