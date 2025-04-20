namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Hosts;

public class HostTests(IntegrationTestWebAppFactory factory)
  : BaseIntegrationTest(factory)
{
  [Fact]
  public async Task CreateHostWithoutUser_WithValidData_ShouldReturnHostIdAsync()
  {
    // Arrange
    var command = new CreateHostWithoutUserCommand(Faker.Name.FullName());

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBe(Guid.Empty);
  }

  [Fact]
  public async Task CreateHostWithoutUser_WithInvalidData_ShouldReturnErrorAsync()
  {
    // Arrange
    var command = new CreateHostWithoutUserCommand(string.Empty);

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }
}
