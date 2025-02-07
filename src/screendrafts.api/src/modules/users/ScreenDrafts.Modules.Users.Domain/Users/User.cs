using Ardalis.GuardClauses;

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
    string? middleName = null)
    : base(id)
  {
    Id = id;
    Email = Guard.Against.Null(email);
    FirstName = Guard.Against.Null(firstName);
    MiddleName = middleName;
    LastName = Guard.Against.Null(lastName);
    IdentityId = identityId;
  }

  private User()
  {
  }

  public Email Email { get; private set; } = default!;

  public FirstName FirstName { get; private set; } = default!;

  public string? MiddleName { get; private set; }

  public LastName LastName { get; private set; } = default!;

  public string IdentityId { get; private set; } = default!;

  public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

  public static User Create(
    Email email,
    FirstName firstName,
    LastName lastName,
    string identityId,
    string? middleName = null,
    UserId? id = null)
  {
    var user = new User(
      email: email,
      firstName: firstName,
      lastName: lastName,
      middleName: middleName,
      identityId: identityId,
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
}
