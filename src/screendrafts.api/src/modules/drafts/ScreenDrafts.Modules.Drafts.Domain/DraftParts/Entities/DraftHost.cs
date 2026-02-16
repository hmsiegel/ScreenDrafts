namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;

public sealed class DraftHost
{
  private DraftHost(
    DraftPart draftPart,
    Host host,
    HostRole role)
  {
    DraftPartId = draftPart.Id;
    DraftPart = draftPart;
    HostId = host.Id;
    Host = host;
    Role = role;
  }

  private DraftHost()
  {
    // EF Core
  }

  public DraftPartId DraftPartId { get; private set; } = default!;
  public DraftPart DraftPart { get; private set; } = default!;

  public HostId HostId { get; private set; } = default!;
  public Host Host { get; private set; } = default!;

  public HostRole Role { get; private set; } = default!;

  public static DraftHost CreatePrimary(DraftPart draftPart, Host host)
  {
    ArgumentNullException.ThrowIfNull(draftPart);
    ArgumentNullException.ThrowIfNull(host);

    return new DraftHost(draftPart, host, HostRole.Primary);
  }

  public static DraftHost CreateCoHost(DraftPart draftPart, Host host)
  {
    ArgumentNullException.ThrowIfNull(draftPart);
    ArgumentNullException.ThrowIfNull(host);
    return new DraftHost(draftPart, host, HostRole.CoHost);
  }
}
