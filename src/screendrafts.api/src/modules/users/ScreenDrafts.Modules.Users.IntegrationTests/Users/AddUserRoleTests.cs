using ScreenDrafts.Modules.Users.Features.Admin.AddRoleToUser;
using ScreenDrafts.Modules.Users.Features.Users.Register;

namespace ScreenDrafts.Modules.Users.IntegrationTests.Users;

public class AddUserRoleTests(UsersIntegrationTestWebAppFactory factory)
  : UsersIntegrationTest(factory)
{
  [Fact]
  public async Task Should_ReturnSuccess_WhenRoleIsAddedAsync()
  {
    // Arrange
    var userId = await Sender.Send(new RegisterUserCommand
    {
        Email = Faker.Internet.Email(),
        Password = Faker.Internet.Password(),
        FirstName = Faker.Name.FirstName(),
        LastName = Faker.Name.LastName()
    });

    var command = new AddRoleToUserCommand(userId.Value, "Host");

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
    var command = new AddRoleToUserCommand(userId, "Host");
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.Errors[0].Should().Be(UserErrors.NotFound(userId));
  }

  [Fact]
  public async Task Should_ReturnError_WhenRoleAlreadyExistsAsync()
  {
    // Arrange
    var userId = await Sender.Send(new RegisterUserCommand
    {
        Email = Faker.Internet.Email(),
        Password = Faker.Internet.Password(),
        FirstName = Faker.Name.FirstName(),
        LastName = Faker.Name.LastName()
    });
    await Sender.Send(new AddRoleToUserCommand(userId.Value, "Host"));
    var command = new AddRoleToUserCommand(userId.Value, "Host");
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.Errors[0].Should().Be(UserErrors.RoleAlreadyExists(userId.Value, "Host"));
  }
}
