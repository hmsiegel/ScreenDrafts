namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class Host : Entity<HostId>
{
  private readonly List<DraftHost> _drafts = [];

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

  public IReadOnlyCollection<DraftHost> Drafts => _drafts.AsReadOnly();

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
}
