namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class Host : Entity<HostId>
{
  private readonly List<Draft> _hostedDrafts = [];

  private Host(
    Guid userId,
    string hostName,
    HostId? id = null)
    : base(id ?? HostId.CreateUnique())
  {
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
    Guid userId,
    string hostName,
    HostId? id = null)
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

  public void RemoveDraft(Draft draft)
  {
    _hostedDrafts.Remove(draft);
  }

  public void UpdateHostName(string firstName, string lastName, string? middleName = null)
  {
    HostName = $"{firstName} {middleName} {lastName}";
  }
}
