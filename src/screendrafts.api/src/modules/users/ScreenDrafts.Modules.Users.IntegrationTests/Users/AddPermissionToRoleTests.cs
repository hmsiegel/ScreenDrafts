namespace ScreenDrafts.Modules.Users.IntegrationTests.Users;

public class AddPermissionToRoleTests(UsersIntegrationTestWebAppFactory factory)
  : UsersIntegrationTest(factory)
{
  [Fact]
  public async Task Should_ReturnSuccess_WhenPermissionIsAddedAsync()
  {
    // Arrange
    // Create a new permission
    var command = new AddPermissionCommand(Faker.Lorem.Word());
    await Sender.Send(command);

    // Get the newly created permission
    var query = new GetPermissionByCodeQuery(command.Code);

    var permissionResult = await Sender.Send(query);

    // Act
    var result = await Sender.Send( new AddPermissionToRoleCommand(
      "Host",
      permissionResult.Value.Code));

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().BeTrue();
  }

  [ Fact]
  public async Task Should_ReturnFailure_WhenPermissionIsAlreadyAddedAsync()
  {
    // Arrange
    // Create a new permission
    var command = new AddPermissionCommand(Faker.Lorem.Word());
    await Sender.Send(command);
    // Get the newly created permission
    var query = new GetPermissionByCodeQuery(command.Code);
    var permissionResult = await Sender.Send(query);
    // Add the permission to the role
    var addPermissionToRoleCommand = new AddPermissionToRoleCommand("Host", permissionResult.Value.Code);
    await Sender.Send(addPermissionToRoleCommand);
    // Act
    var result = await Sender.Send(addPermissionToRoleCommand);
    // Assert
    result.IsFailure.Should().BeTrue();
  }
}
