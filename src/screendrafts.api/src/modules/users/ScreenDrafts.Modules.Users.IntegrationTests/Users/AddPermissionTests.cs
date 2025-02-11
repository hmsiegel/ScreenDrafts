namespace ScreenDrafts.Modules.Users.IntegrationTests.Users;

public class AddPermissionTests(IntegrationTestWebAppFactory factory)
  : BaseIntegrationTest(factory)
{
  [Fact]
  public async Task Should_ReturnSuccess_WhenPermissionIsAddedAsync()
  {
    // Arrange
    var command = new AddPermissionCommand(
      Faker.Lorem.Word());

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().BeTrue();
  }

  [Fact]
  public async Task Should_ReturnError_WhenPermissionAlreadyExistsAsync()
  {
    // Arrange
    var command = new AddPermissionCommand(
      Faker.Lorem.Word());
    await Sender.Send(command);
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.Errors[0].Should().Be(UserErrors.PermissionAlreadyExists(command.Code));
  }
}
