namespace ScreenDrafts.Modules.Drafts.Domain.People;

public sealed class Person : AggregateRoot<PersonId, Guid>
{
  private Person(
    string publicId,
    string firstName,
    string lastName,
    PersonId? id = null,
    string? displayName = null,
    Guid? userId = null)
    : base(id ?? PersonId.CreateUnique())
  {
    UserId = userId;
    FirstName = firstName;
    LastName = lastName;
    PublicId = publicId;
    DisplayName = string.IsNullOrWhiteSpace(displayName)
      ? FullName
      : displayName;
  }

  private Person()
  {
  }

  public Guid? UserId { get; private set; }

  public string PublicId { get; private set; } = default!;

  public string FirstName { get; private set; } = default!;

  public string LastName { get; private set; } = default!;

  public string? DisplayName { get; private set; }

  public Drafter? DrafterProfile { get; private set; } = default!;
  public Host? HostProfile { get; private set; } = default!;

  public string FullName => $"{FirstName} {LastName}".Trim();

  public static Result<Person> Create(
    string publicId,
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
      publicId: publicId,
      userId: userId,
      firstName: firstName,
      lastName: lastName,
      displayName: displayName);

    person.Raise(new PersonCreatedDomainEvent(person.Id.Value, publicId));

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

  public Result Update(
    string firstName,
    string lastName,
    string? displayName = null)
  {
    if (string.IsNullOrWhiteSpace(firstName))
    {
      return Result.Failure(PersonErrors.InvalidFirstName);
    }

    if (string.IsNullOrWhiteSpace(lastName))
    {
      return Result.Failure(PersonErrors.InvalidLastName);
    }

    var normalizdDisplayName =
      string.IsNullOrWhiteSpace(displayName)
      ? $"{firstName} {lastName}".Trim()
      : displayName;

    if (FirstName == firstName &&
      LastName == lastName &&
      DisplayName == normalizdDisplayName)
    {
      return Result.Success();
    }

    FirstName = firstName;
    LastName = lastName;
    DisplayName = normalizdDisplayName;
    Raise(new PersonUpdatedDomainEvent(Id.Value));
    return Result.Success();
  }
}
