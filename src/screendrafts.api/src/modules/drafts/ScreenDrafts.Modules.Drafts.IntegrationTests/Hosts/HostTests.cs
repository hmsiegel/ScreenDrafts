namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Hosts;

public class HostTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task CreateHostWithoutUser_WithValidData_ShouldReturnHostIdAsync()
  {
    // Arrange
    var personFactory = new PeopleFactory(Sender, Faker);
    var personId = await personFactory.CreateAndSavePersonAsync();
    var command = new CreateHostCommand(personId);

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
    var command = new CreateHostCommand(Guid.Empty);

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }
}
