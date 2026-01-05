namespace ScreenDrafts.Modules.Drafts.Domain.People;

public sealed class Person : AggrgateRoot<PersonId, Guid>
{
  private Person(
    string firstName,
    string lastName,
    PersonId? id = null,
    string? displayName = null,
    Guid? userId = null)
    : base(id ?? PersonId.CreateUnique())
  {
    UserId = userId;
    FirstName = Guard.Against.NullOrWhiteSpace(firstName);
    LastName = Guard.Against.NullOrWhiteSpace(lastName);
    DisplayName = displayName ?? FullName;
  }

  private Person()
  {
  }

  public Guid? UserId { get; private set; }

  public string FirstName { get; private set; } = default!;

  public string LastName { get; private set; } = default!;

  public string? DisplayName { get; set; }

  public Drafter? DrafterProfile { get; private set; } = default!;
  public Host? HostProfile { get; private set; } = default!;

  public string FullName => $"{FirstName} {LastName}".Trim();

  public static Result<Person> Create(
    string firstName,
    string lastName,
    Guid? userId = null,
    string? displayName = null,
    Guid? id = null)
  {
    if (string.IsNullOrWhiteSpace(firstName))
    {
      return Result.Failure<Person>(PersonErrors.InvalidFirstName);
    }
    if (string.IsNullOrWhiteSpace(lastName))
    {
      return Result.Failure<Person>(PersonErrors.InvalidLastName);
    }
    var person = new Person(
      id: id is not null ? PersonId.Create(id.Value) : null,
      userId: userId,
      firstName: firstName,
      lastName: lastName,
      displayName: displayName);

    person.Raise(new PersonCreatedDomainEvent(person.Id.Value));

    return person;
  }

  public Result AssignUserId(Guid userId)
  {
    if (userId == Guid.Empty)
    {
      return Result.Failure(PersonErrors.UserIdCannotBeEmpty);
    }

    if (UserId is not null)
    {
      return Result.Failure(PersonErrors.UserAlreadyAssigned(userId));
    }

    UserId = userId;

    return Result.Success();
  }
}
