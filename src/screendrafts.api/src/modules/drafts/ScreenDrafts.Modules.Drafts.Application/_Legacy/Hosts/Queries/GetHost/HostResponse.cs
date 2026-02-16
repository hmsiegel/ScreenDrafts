namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Hosts.Queries.GetHost;

public sealed record HostResponse(
  Guid Id,
  Guid PersonId,
  string FirstName,
  string LastName,
  string DisplayName)
{
  public HostResponse()
      : this(
          Guid.Empty,
          Guid.Empty,
          string.Empty,
          string.Empty,
          string.Empty)
  {
  }
}
