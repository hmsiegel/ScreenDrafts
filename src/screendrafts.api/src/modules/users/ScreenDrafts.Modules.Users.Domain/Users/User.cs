namespace ScreenDrafts.Modules.Users.Domain.Users;

public sealed class User : AggregateRoot<UserId, Guid>
{
  private User(
    UserId id,
    Email email,
    FirstName firstName,
    LastName lastName,
    string identityId,
    string publicId,
    Guid? personId,
    string? personPublicId,
    string? middleName = null
  )
    : base(id)
  {
    Id = id;
    Email = Guard.Against.Null(email);
    FirstName = Guard.Against.Null(firstName);
    MiddleName = middleName;
    LastName = Guard.Against.Null(lastName);
    IdentityId = identityId;
    PublicId = publicId;
    PersonId = personId;
    PersonPublicId = personPublicId;
  }

  private User() { }

  public string PublicId { get; private set; } = default!;
  public Email Email { get; private set; } = default!;
  public FirstName FirstName { get; private set; } = default!;
  public string? MiddleName { get; private set; }
  public LastName LastName { get; private set; } = default!;
  public string IdentityId { get; private set; } = default!;
  public Guid? PersonId { get; private set; } = default!;
  public string? PersonPublicId { get; private set; } = default!;

  public static Result<User> Create(
    Email email,
    FirstName firstName,
    LastName lastName,
    string identityId,
    string publicId,
    string? middleName = null,
    UserId? id = null,
    Guid? personId = null,
    string? personPublicId = null
  )
  {
    var user = new User(
      email: email,
      firstName: firstName,
      lastName: lastName,
      middleName: middleName,
      identityId: identityId,
      personId: personId,
      personPublicId: personPublicId,
      publicId: publicId,
      id: id ?? UserId.CreateUnique()
    );

    user.Raise(new UserRegisteredDomainEvent(user.Id.Value));

    return user;
  }

  public void Update(FirstName firstName, LastName lastName, string? middleName = null)
  {
    ArgumentNullException.ThrowIfNull(firstName);
    ArgumentNullException.ThrowIfNull(lastName);

    if (FirstName == firstName && LastName == lastName && MiddleName == middleName)
    {
      return;
    }

    FirstName = firstName;
    LastName = lastName;
    MiddleName = middleName;

    Raise(new UserProfileUpdatedDomainEvent(Id.Value, firstName.Value!, lastName.Value!));
  }

  public void LinkPerson(Guid personId, string personPublicId)
  {
    if (PersonId == personId && PersonPublicId == personPublicId)
    {
      return;
    }
    PersonId = personId;
    PersonPublicId = personPublicId;

    Raise(new UserLinkedToPersonDomainEvent(Id.Value, personId, personPublicId));
  }

  internal void SetPublicId(string publicId)
  {
    ArgumentNullException.ThrowIfNull(publicId);
    if (PublicId == publicId)
    {
      return;
    }
    PublicId = publicId;
  }
}
