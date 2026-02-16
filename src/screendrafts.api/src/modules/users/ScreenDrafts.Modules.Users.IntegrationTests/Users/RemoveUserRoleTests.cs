namespace ScreenDrafts.Modules.Users.IntegrationTests.Users;
public class RemoveUserRoleTests(UsersIntegrationTestWebAppFactory factory)
  : UsersIntegrationTest(factory)
{
  [Fact]
  public async Task Should_ReturnSuccess_WhenRoleIsRemovedAsync()
  {
    var userId = await Sender.Send(new Features.Users.Register.RegisterUserCommand
    {
        Email = Faker.Internet.Email(),
        Password = Faker.Internet.Password(),
        FirstName = Faker.Name.FirstName(),
        LastName = Faker.Name.LastName()
    });

    await Sender.Send(new Features.Admin.AddRoleToUser.AddRoleToUserCommand(userId.Value, "Host"));


    var command = new Features.Admin.RemoveRoleFromUser.RemoveRoleFromUserCommand(userId.Value, "Host");

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
    var command = new Features.Admin.RemoveRoleFromUser.RemoveRoleFromUserCommand(userId, "Host");
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.Errors[0].Should().Be(UserErrors.NotFound(userId));
  }

  [Fact]
  public async Task Should_ReturnError_WhenRoleDoesNotExistAsync()
  {
    // Arrange
    var userId = await Sender.Send(new Features.Users.Register.RegisterUserCommand
    {
      Email = Faker.Internet.Email(),
      Password = Faker.Internet.Password(),
      FirstName = Faker.Name.FirstName(),
      LastName = Faker.Name.LastName()
    });
    var command = new Features.Admin.RemoveRoleFromUser.RemoveRoleFromUserCommand(userId.Value, "Host");
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.Errors[0].Should().Be(UserErrors.RoleDoesNotExist);
  }

  [Fact]
  public async Task Should_ReturnError_WhenCannotRemoveLastRoleAsync()
  {
    // Arrange
    var userId = await Sender.Send(new Features.Users.Register.RegisterUserCommand
    {
      Email = Faker.Internet.Email(),
      Password = Faker.Internet.Password(),
      FirstName = Faker.Name.FirstName(),
      LastName = Faker.Name.LastName()
    });
    var command = new Features.Admin.RemoveRoleFromUser.RemoveRoleFromUserCommand(userId.Value, "Guest");
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.Errors[0].Should().Be(UserErrors.CannotRemoveLastRole);
  }
}
