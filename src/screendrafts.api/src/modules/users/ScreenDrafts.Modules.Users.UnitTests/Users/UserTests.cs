namespace ScreenDrafts.Modules.Users.UnitTests.Users;

public class UserTests : BaseTest
{
  [Fact]
  public void Create_ShouldReturnUser()
  {
    // Act
    var user = User.Create(
        Email.Create(Faker.Internet.Email()).Value,
        FirstName.Create(Faker.Name.FirstName()).Value,
        LastName.Create(Faker.Name.LastName()).Value,
        Guid.NewGuid().ToString());

    // Assert
    user.Should().NotBeNull();
  }

  [Fact]
  public void Create_ShouldRaiseDomainEvent_WhenUserCreated()
  {
    // Act
    var user = User.Create(
        Email.Create(Faker.Internet.Email()).Value,
        FirstName.Create(Faker.Name.FirstName()).Value,
        LastName.Create(Faker.Name.LastName()).Value,
        Guid.NewGuid().ToString());

    // Assert
    UserRegisteredDomainEvent domainEvent =
        AssertDomainEventWasPublished<UserRegisteredDomainEvent>(user);

    domainEvent.UserId.Should().Be(user.Id.Value);
  }

  [Fact]
  public void Update_ShouldRaiseDomainEvent_WhenUserUpdated()
  {
    // Arrange
    var user = User.Create(
        Email.Create(Faker.Internet.Email()).Value,
        FirstName.Create(Faker.Name.FirstName()).Value,
        LastName.Create(Faker.Name.LastName()).Value,
        Guid.NewGuid().ToString());

    var firstName = FirstName.Create(Faker.Name.FirstName()).Value;
    var lastName = LastName.Create(Faker.Name.LastName()).Value;

    // Act
    user.Update(firstName, lastName);

    // Assert
    UserProfileUpdatedDomainEvent domainEvent =
        AssertDomainEventWasPublished<UserProfileUpdatedDomainEvent>(user);

    domainEvent.UserId.Should().Be(user.Id.Value);
    domainEvent.FirstName.Should().Be(user.FirstName.Value);
    domainEvent.LastName.Should().Be(user.LastName.Value);
  }

  [Fact]
  public void Update_ShouldNotRaiseDomainEvent_WhenUserNotUpdated()
  {
    // Arrange
    var user = User.Create(
        Email.Create(Faker.Internet.Email()).Value,
        FirstName.Create(Faker.Name.FirstName()).Value,
        LastName.Create(Faker.Name.LastName()).Value,
        Guid.NewGuid().ToString());

    user.ClearDomainEvents();

    // Act
    user.Update(user.FirstName, user.LastName);

    // Assert
    user.DomainEvents.Should().BeEmpty();
  }
}
