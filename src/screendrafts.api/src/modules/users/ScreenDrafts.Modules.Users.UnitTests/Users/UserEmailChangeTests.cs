namespace ScreenDrafts.Modules.Users.UnitTests.Users;

public class UserEmailChangeTests : BaseTest
{
  private readonly string _publicId = $"u_{Nanoid.Generate(size: 15)}";

  private Result<User> CreateUser(string? email = null) =>
    User.Create(
      Email.Create(email ?? Faker.Internet.Email()).Value,
      FirstName.Create(Faker.Name.FirstName()).Value,
      LastName.Create(Faker.Name.LastName()).Value,
      Guid.NewGuid().ToString(),
      _publicId
    );

  [Fact]
  public void ChangeEmail_WhenEmailDiffers_ShouldRaiseDomainEventWithCorrectPayload()
  {
    // Arrange
    var user = CreateUser().Value;
    user.ClearDomainEvents();
    var newEmail = Email.Create(Faker.Internet.Email()).Value;

    // Act
    user.ChangeEmail(newEmail);

    // Assert
    user.Email.Should().Be(newEmail);
    var domainEvent = AssertDomainEventWasPublished<UserEmailChangedDomainEvent>(user);
    domainEvent.UserId.Should().Be(user.Id.Value);
    domainEvent.NewEmail.Should().Be(newEmail.Value);
  }

  [Fact]
  public void ChangeEmail_WhenEmailSame_ShouldNotRaiseDomainEvent()
  {
    // Arrange
    var email = Faker.Internet.Email();
    var user = CreateUser(email).Value;
    user.ClearDomainEvents();

    // Act
    user.ChangeEmail(Email.Create(email).Value);

    // Assert
    user.DomainEvents.Should().BeEmpty();
  }

  [Fact]
  public void RequestEmailChange_ShouldRaiseDomainEventWithCorrectPayload_AndNotChangeEmail()
  {
    // Arrange
    var user = CreateUser().Value;
    var originalEmail = user.Email;
    user.ClearDomainEvents();
    var newEmail = Faker.Internet.Email();
    const string confirmationLink = "https://screendrafts.example/confirm?token=abc123";

    // Act
    user.RequestEmailChange(newEmail, confirmationLink);

    // Assert
    user.Email.Should().Be(originalEmail);
    var domainEvent = AssertDomainEventWasPublished<UserEmailChangeRequestedDomainEvent>(user);
    domainEvent.UserId.Should().Be(user.Id.Value);
    domainEvent.NewEmail.Should().Be(newEmail);
    domainEvent.ConfirmationLink.Should().Be(confirmationLink);
  }
}
