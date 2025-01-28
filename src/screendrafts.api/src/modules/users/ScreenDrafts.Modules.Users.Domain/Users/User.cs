namespace ScreenDrafts.Modules.Users.Domain.Users;

public sealed class User : AggrgateRoot<UserId, Guid>
{
  private User(
    UserId id,
    Email email,
    FirstName firstName,
    LastName lastName,
    string? middleName = null)
    : base (id)
  {
    Id = id; 
    Email = email;
    FirstName = firstName;
    MiddleName = middleName;
    LastName = lastName;
  }

  private User()
  {
  }

  public Email Email { get; private set; } = default!;

  public FirstName FirstName { get; private set; } = default!;

  public string? MiddleName { get; private set; }

  public LastName LastName { get; private set; } = default!;

  public static User Create(
    Email email,
    FirstName firstName,
    LastName lastName,
    string? middleName = null,
    UserId? id = null)
  {
    var user = new User(
      email: email,
      firstName: firstName,
      lastName: lastName,
      middleName: middleName,
      id: id ?? UserId.CreateUnique());

    user.Raise(new UserCreatedDomainEvent(user.Id.Value));

    return user;
  }

  public void Update(
    FirstName firstName,
    LastName lastName,
    string? middleName = null)
  {
    if (FirstName == firstName && LastName == lastName && MiddleName == middleName)
    {
      return;
    }

    FirstName = firstName;
    LastName = lastName;
    MiddleName = middleName;

    Raise(new UserUpdatedDomainEvent(Id.Value));
  }
}
