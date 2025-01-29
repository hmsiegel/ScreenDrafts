namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class Host : Entity
{
  private readonly List<Draft> _hostedDrafts = [];

  private Host(
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

  public static Result<Host> Create(
    Guid id,
    Guid userId,
    string hostName)
  {
    if (string.IsNullOrWhiteSpace(hostName))
    {
      return Result.Failure<Host>(HostErrors.InvalidHostName);
    }
    var host = new Host(
      id: id,
      userId: userId,
      hostName: hostName);
    return host;
  }

  public void AddDraft(Draft draft)
  {
    _hostedDrafts.Add(draft);
  }
}

