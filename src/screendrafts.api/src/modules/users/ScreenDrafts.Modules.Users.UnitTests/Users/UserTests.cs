using NanoidDotNet;

namespace ScreenDrafts.Modules.Users.UnitTests.Users;

public class UserTests : BaseTest
{
  private readonly string _publicId = $"u_{Nanoid.Generate(size: 15)}";

  [Fact]
  public void Create_ShouldReturnUser()
  {
    // Act
    var user = User.Create(
        Email.Create(Faker.Internet.Email()).Value,
        FirstName.Create(Faker.Name.FirstName()).Value,
        LastName.Create(Faker.Name.LastName()).Value,
        Guid.NewGuid().ToString(),
        _publicId);

    // Assert
    user.Value.Should().NotBeNull();
  }

  [Fact]
  public void Create_ShouldRaiseDomainEvent_WhenUserCreated()
  {
    // Act
    var user = User.Create(
        Email.Create(Faker.Internet.Email()).Value,
        FirstName.Create(Faker.Name.FirstName()).Value,
        LastName.Create(Faker.Name.LastName()).Value,
        Guid.NewGuid().ToString(),
        _publicId);

    // Assert
    UserRegisteredDomainEvent domainEvent =
        AssertDomainEventWasPublished<UserRegisteredDomainEvent>(user.Value);

    domainEvent.UserId.Should().Be(user.Value.Id.Value);
  }

  [Fact]
  public void Update_ShouldRaiseDomainEvent_WhenUserUpdated()
  {
    // Arrange
    var user = User.Create(
        Email.Create(Faker.Internet.Email()).Value,
        FirstName.Create(Faker.Name.FirstName()).Value,
        LastName.Create(Faker.Name.LastName()).Value,
        Guid.NewGuid().ToString(),
        _publicId);

    var firstName = FirstName.Create(Faker.Name.FirstName()).Value;
    var lastName = LastName.Create(Faker.Name.LastName()).Value;

    // Act
    user.Value.Update(firstName, lastName);

    // Assert
    UserProfileUpdatedDomainEvent domainEvent =
        AssertDomainEventWasPublished<UserProfileUpdatedDomainEvent>(user.Value);

    domainEvent.UserId.Should().Be(user.Value.Id.Value);
    domainEvent.FirstName.Should().Be(user.Value.FirstName.Value);
    domainEvent.LastName.Should().Be(user.Value.LastName.Value);
  }

  [Fact]
  public void Update_ShouldNotRaiseDomainEvent_WhenUserNotUpdated()
  {
    // Arrange
    var user = User.Create(
        Email.Create(Faker.Internet.Email()).Value,
        FirstName.Create(Faker.Name.FirstName()).Value,
        LastName.Create(Faker.Name.LastName()).Value,
        Guid.NewGuid().ToString(),
        _publicId);

    user.Value.ClearDomainEvents();

    // Act
    user.Value.Update(user.Value.FirstName, user.Value.LastName);

    // Assert
    user.Value.DomainEvents.Should().BeEmpty();
  }
}
