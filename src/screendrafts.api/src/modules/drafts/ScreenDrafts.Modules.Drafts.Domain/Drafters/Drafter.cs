namespace ScreenDrafts.Modules.Drafts.Domain.Drafters;

public sealed class Drafter : AggregateRoot<DrafterId, Guid>
{
  private Drafter(
    Person person,
    string publicId,
    DrafterId? id = null)
    : base(id ?? DrafterId.CreateUnique())
  {
    Person = person;
    PublicId = publicId;
    PersonId = person.Id;
  }

  private Drafter()
  {
  }

  public string PublicId { get; init; } = default!;

  public Person Person { get; private set; } = default!;

  public PersonId PersonId { get; private set; } = default!;

  public bool IsRetired { get; private set; } = default!;

  public DateTime? RetiredAtUtc { get; private set; } = default!;

  public static Result<Drafter> Create(
    Person person,
    string publicId,
    DrafterId? id = null)
  {
    ArgumentNullException.ThrowIfNull(person);

    var drafter = new Drafter(
      id: id,
      publicId: publicId,
      person: person);

    drafter.Raise(new DrafterCreatedDomainEvent(drafter.Id.Value));

    return drafter;
  }

  public Result RetireDrafter()
  {
    if (IsRetired)
    {
      return Result.Failure(DrafterErrors.AlreadyRetired);
    }

    IsRetired = true;
    RetiredAtUtc = DateTime.UtcNow;

    return Result.Success();
  }
}
