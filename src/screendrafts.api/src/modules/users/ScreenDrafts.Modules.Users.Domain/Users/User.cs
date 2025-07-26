namespace ScreenDrafts.Modules.Users.Domain.Users;
public sealed class User : AggrgateRoot<UserId, Guid>
{
  private readonly List<Role> _roles = [];

  private User(
    UserId id,
    Email email,
    FirstName firstName,
    LastName lastName,
    string identityId,
    Guid? personId,
    string? middleName = null)
    : base(id)
  {
    Id = id;
    Email = Guard.Against.Null(email);
    FirstName = Guard.Against.Null(firstName);
    MiddleName = middleName;
    LastName = Guard.Against.Null(lastName);
    IdentityId = identityId;
    PersonId = personId;
  }

  private User()
  {
  }

  public Email Email { get; private set; } = default!;

  public FirstName FirstName { get; private set; } = default!;

  public string? MiddleName { get; private set; }

  public LastName LastName { get; private set; } = default!;

  public string IdentityId { get; private set; } = default!;

  public Guid? PersonId { get; private set; } = default!;

  public string? ProfilePicturePath { get; private set; } = default!;

  public string? TwitterHandle { get; private set; } = default!;

  public string? InstagramHandle { get; private set; } = default!;

  public string? LetterboxdHandle { get; private set; } = default!;

  public string? BlueskyHandle { get; private set; } = default!;

  public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

  public static Result<User> Create(
    Email email,
    FirstName firstName,
    LastName lastName,
    string identityId,
    string? middleName = null,
    UserId? id = null,
    Guid? personId = null)
  {
    var user = new User(
      email: email,
      firstName: firstName,
      lastName: lastName,
      middleName: middleName,
      identityId: identityId,
      personId: personId,
      id: id ?? UserId.CreateUnique());

    user._roles.Add(Role.Guest);

    user.Raise(new UserRegisteredDomainEvent(user.Id.Value));

    return user;
  }

  public void Update(
    FirstName firstName,
    LastName lastName,
    string? middleName = null)
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

    Raise(new UserProfileUpdatedDomainEvent(
      Id.Value,
      firstName.Value!,
      lastName.Value!));
  }

  public void UpdateProfilePicture(string relativePath)
  {
    ArgumentNullException.ThrowIfNull(relativePath);
    if (ProfilePicturePath == relativePath)
    {
      return;
    }
    ProfilePicturePath = relativePath;
    Raise(new UserProfilePictureUpdatedDomainEvent(Id.Value, relativePath));
  }

  public void UpdateSocialHandles(
    string? twitterHandle = null,
    string? instagramHandle = null,
    string? letterboxdHandle = null,
    string? blueskyHandle = null)
  {
    TwitterHandle = twitterHandle;
    InstagramHandle = instagramHandle;
    LetterboxdHandle = letterboxdHandle;
    BlueskyHandle = blueskyHandle;
    Raise(new UserSocialHandlesUpdatedDomainEvent(
      Id.Value,
      twitterHandle,
      instagramHandle,
      letterboxdHandle,
      blueskyHandle));
  }

  public void AddRole(Role role)
  {
    ArgumentNullException.ThrowIfNull(role);
    if (_roles.Contains(role))
    {
      return;
    }
    _roles.Add(role);
    Raise(new UserRoleAddedDomainEvent(Id.Value, role.Name));
  }

  public void ClearRoles() => _roles.Clear();
}
