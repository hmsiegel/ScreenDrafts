namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class Host : Entity
{
  private readonly List<Draft> _hostedDrafts = [];

  public Host(
    Guid id,
    Guid userId,
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


  public Guid UserId { get; private set; }

  public string HostName { get; private set; } = default!;

  public IReadOnlyCollection<Draft> HostedDrafts => _hostedDrafts.AsReadOnly();
}

