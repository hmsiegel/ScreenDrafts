namespace ScreenDrafts.Modules.Drafts.Domain.Hosts;

public sealed class Host : Entity<HostId>
{
  private readonly List<DraftHost> _drafts = [];

  private Host(Person person, string publicId, HostId? id = null)
    : base(id ?? HostId.CreateUnique())
  {
    Person = person;
    PublicId = publicId;
    PersonId = person.Id;
  }

  private Host() { }

  public string PublicId { get; private set; } = default!;
  public Person Person { get; private set; } = default!;
  public PersonId PersonId { get; private set; } = default!;
  public bool IsRetired { get; private set; }
  public DateTime? RetiredAtUtc { get; private set; }

  public IReadOnlyCollection<DraftHost> Drafts => _drafts.AsReadOnly();

  public static Result<Host> Create(Person person, string publicId, HostId? id = null)
  {
    ArgumentNullException.ThrowIfNull(person);

    var host = new Host(id: id, publicId: publicId, person: person);

    host.Raise(new HostCreatedDomainEvent(host.Id.Value, host.PersonId.Value, host.PublicId));

    return host;
  }

  public Result Retire()
  {
    if (IsRetired)
    {
      return Result.Failure(HostErrors.AlreadyRetired);
    }

    IsRetired = true;
    RetiredAtUtc = DateTime.UtcNow;

    return Result.Success();
  }
}
