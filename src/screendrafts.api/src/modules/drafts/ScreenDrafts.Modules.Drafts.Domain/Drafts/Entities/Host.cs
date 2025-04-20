namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class Host : Entity<HostId>
{
  private readonly List<Draft> _hostedDrafts = [];

  private Host(
    string hostName,
    Guid? userId = null,
    HostId? id = null)
    : base(id ?? HostId.CreateUnique())
  {
    UserId = userId;
    HostName = hostName;
  }

  private Host()
  {
  }

  public int ReadableId { get; init; }

  public Guid? UserId { get; private set; }

  public string HostName { get; private set; } = default!;

  public IReadOnlyCollection<Draft> HostedDrafts => _hostedDrafts.AsReadOnly();

  public static Result<Host> Create(
    string hostName,
    Guid? userId = null,
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

  public Result AssignUserId(Guid userId)
  {
    if (userId == Guid.Empty)
    {
      return Result.Failure(HostErrors.UserIdCannotBeEmpty);
    }

    UserId = userId;
    return Result.Success();
  }

  public Result AddDraft(Draft draft)
  {
    if (draft is null)
    {
      return Result.Failure(HostErrors.DraftCannotBeNull);
    }

    ArgumentNullException.ThrowIfNull(draft);

    _hostedDrafts.Add(draft);

    return Result.Success();
  }

  public Result RemoveDraft(Draft draft)
  {
    if (draft is null)
    {
      return Result.Failure(HostErrors.DraftCannotBeNull);
    }

    if (!_hostedDrafts.Contains(draft))
    {
      return Result.Failure(HostErrors.DraftCannotBeNull);
    }

    ArgumentNullException.ThrowIfNull(draft);
    _hostedDrafts.Remove(draft);
    return Result.Success();
  }

  public Result UpdateHostName(string newName)
  {
    if (string.IsNullOrWhiteSpace(newName))
    {
      return Result.Failure(HostErrors.InvalidHostName);
    }

    HostName = newName;
    return Result.Success();
  }
}
