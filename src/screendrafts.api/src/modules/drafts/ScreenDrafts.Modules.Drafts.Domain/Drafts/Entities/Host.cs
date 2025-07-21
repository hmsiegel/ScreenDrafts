namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class Host : Entity<HostId>
{
  private readonly List<Draft> _hostedDrafts = [];

  private Host(
    Person person,
    HostId? id = null)
    : base(id ?? HostId.CreateUnique())
  {
    Person = person;
    PersonId = person.Id;
  }

  private Host()
  {
  }

  public int ReadableId { get; init; }

  public Person Person { get; private set; } = default!;

  public PersonId PersonId { get; private set; } = default!;

  public IReadOnlyCollection<Draft> HostedDrafts => _hostedDrafts.AsReadOnly();

  public static Result<Host> Create(
    Person person,
    HostId? id = null)
  {
    ArgumentNullException.ThrowIfNull(person);

    var host = new Host(
      id: id,
      person: person);
    return host;
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
}
