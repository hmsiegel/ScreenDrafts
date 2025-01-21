namespace ScreenDrafts.Modules.Drafts.Domain.Drafters;

public sealed class Host : Entity
{
  public Host(
    Ulid id,
    Ulid userId,
    string hostName)
    : base(id)
  {
    Id = id;
    UserId = userId;
    HostName = hostName;
  }

  private Host()
  {
  }


  public Ulid UserId { get; private set; }

  public string HostName { get; private set; } = default!;
}

