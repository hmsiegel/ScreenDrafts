namespace ScreenDrafts.Modules.Users.IntegrationTests.Users;

public class AddUserRoleTests(UsersIntegrationTestWebAppFactory factory)
  : UsersIntegrationTest(factory)
{
  [Fact]
  public async Task Should_ReturnSuccess_WhenRoleIsAddedAsync()
  {
    // Arrange
    var userId = await Sender.Send(new RegisterUserCommand(
        Faker.Internet.Email(),
        Faker.Internet.Password(),
        Faker.Name.FirstName(),
        Faker.Name.LastName()));

    var command = new AddUserRoleCommand(userId.Value, "Host");

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
    var command = new AddUserRoleCommand(userId, "Host");
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.Errors[0].Should().Be(UserErrors.NotFound(userId));
  }

  [Fact]
  public async Task Should_ReturnError_WhenRoleAlreadyExistsAsync()
  {
    // Arrange
    var userId = await Sender.Send(new RegisterUserCommand(
        Faker.Internet.Email(),
        Faker.Internet.Password(),
        Faker.Name.FirstName(),
        Faker.Name.LastName()));
    await Sender.Send(new AddUserRoleCommand(userId.Value, "Host"));
    var command = new AddUserRoleCommand(userId.Value, "Host");
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.Errors[0].Should().Be(UserErrors.RoleAlreadyExists(userId.Value, "Host"));
  }
}
