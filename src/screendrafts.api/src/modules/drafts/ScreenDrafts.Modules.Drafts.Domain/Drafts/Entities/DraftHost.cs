namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class DraftHost
{
  private DraftHost(
    Draft draft,
    Host host,
    HostRole role)
  {
    DraftId = draft.Id;
    Draft = draft;
    HostId = host.Id;
    Host = host;
    Role = role;
  }

  private DraftHost()
  {
    // EF Core
  }

  public DraftId DraftId { get; private set; } = default!;
  public Draft Draft { get; private set; } = default!;

  public HostId HostId { get; private set; } = default!;
  public Host Host { get; private set; } = default!;

  public HostRole Role { get; private set; } = default!;

  public static DraftHost CreatePrimary(Draft draft, Host host)
  {
    ArgumentNullException.ThrowIfNull(draft);
    ArgumentNullException.ThrowIfNull(host);

    return new DraftHost(draft, host, HostRole.Primary);
  }

  public static DraftHost CreateCoHost(Draft draft, Host host)
  {
    ArgumentNullException.ThrowIfNull(draft);
    ArgumentNullException.ThrowIfNull(host);
    return new DraftHost(draft, host, HostRole.CoHost);
  }
}
