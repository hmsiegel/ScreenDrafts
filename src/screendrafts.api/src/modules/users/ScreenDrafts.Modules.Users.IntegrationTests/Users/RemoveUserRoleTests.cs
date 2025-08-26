using ScreenDrafts.Modules.Users.Application.Users.Commands.RemoveUserRole;

namespace ScreenDrafts.Modules.Users.IntegrationTests.Users;
public class RemoveUserRoleTests(UsersIntegrationTestWebAppFactory factory)
  : UsersIntegrationTest(factory)
{
  [Fact]
  public async Task Should_ReturnSuccess_WhenRoleIsRemovedAsync()
  {
    var userId = await Sender.Send(new RegisterUserCommand(
        Faker.Internet.Email(),
        Faker.Internet.Password(),
        Faker.Name.FirstName(),
        Faker.Name.LastName()));

    await Sender.Send(new AddUserRoleCommand(userId.Value, "Host"));


    var command = new RemoveUserRoleCommand(userId.Value, "Host");

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().BeTrue();
  }

  [Fact]
  public async Task Should_ReturnError_WhenUserDoesNotExistAsync()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var command = new RemoveUserRoleCommand(userId, "Host");
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.Errors[0].Should().Be(UserErrors.NotFound(userId));
  }

  [Fact]
  public async Task Should_ReturnError_WhenRoleDoesNotExistAsync()
  {
    // Arrange
    var userId = await Sender.Send(new RegisterUserCommand(
        Faker.Internet.Email(),
        Faker.Internet.Password(),
        Faker.Name.FirstName(),
        Faker.Name.LastName()));
    var command = new RemoveUserRoleCommand(userId.Value, "Host");
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.Errors[0].Should().Be(UserErrors.RoleDoesNotExist);
  }

  [Fact]
  public async Task Should_ReturnError_WhenCannotRemoveLastRoleAsync()
  {
    // Arrange
    var userId = await Sender.Send(new RegisterUserCommand(
        Faker.Internet.Email(),
        Faker.Internet.Password(),
        Faker.Name.FirstName(),
        Faker.Name.LastName()));
    var command = new RemoveUserRoleCommand(userId.Value, "Guest");
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.Errors[0].Should().Be(UserErrors.CannotRemoveLastRole);
  }
}
