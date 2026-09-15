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
    string? middleName = null,
    bool isSocialLogin = false
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
    IsSocialLogin = isSocialLogin;
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

  /// <summary>
  /// True if this account was created through a social/federated login
  /// (Register/Social) rather than local email+password registration. Fixed
  /// at creation — never changes after the fact, so there's no setter.
  /// Existing accounts default to false.
  /// </summary>
  public bool IsSocialLogin { get; private set; }

  public static Result<User> Create(
    Email email,
    FirstName firstName,
    LastName lastName,
    string identityId,
    string publicId,
    string? middleName = null,
    UserId? id = null,
    Guid? personId = null,
    string? personPublicId = null,
    bool isSocialLogin = false
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
      id: id ?? UserId.CreateUnique(),
      isSocialLogin: isSocialLogin
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

  /// <summary>
  /// Updates the module's own copy of the email — this must be called any time
  /// the Keycloak-side email changes (bootstrap claim, or steady-state confirm),
  /// or the app keeps showing/using the old address while Keycloak has the new one.
  /// </summary>
  public void ChangeEmail(Email newEmail)
  {
    ArgumentNullException.ThrowIfNull(newEmail);

    if (Email == newEmail)
    {
      return;
    }

    Email = newEmail;

    Raise(new UserEmailChangedDomainEvent(Id.Value, newEmail.Value!));
  }

  /// <summary>
  /// Steady-state email change, step 1: records intent and raises the event
  /// that triggers the confirmation email. Does not mutate Email — that only
  /// happens once the user confirms via ChangeEmail.
  /// </summary>
  public void RequestEmailChange(string newEmail, string confirmationLink)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(newEmail);
    ArgumentException.ThrowIfNullOrWhiteSpace(confirmationLink);

    Raise(new UserEmailChangeRequestedDomainEvent(Id.Value, newEmail, confirmationLink));
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
