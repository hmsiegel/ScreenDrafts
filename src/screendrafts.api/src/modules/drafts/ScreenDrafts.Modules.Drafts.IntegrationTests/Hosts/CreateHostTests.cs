namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Hosts;

public sealed class CreateHostTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task CreateHost_WithValidRequest_ShouldReturnHostAsync()
  {
    // Arrange
    var personFactory = new PeopleFactory(Sender, Faker);
    var personId = await personFactory.CreateAndSavePersonAsync();
    var command = new CreateHostCommand(personId);
    // Act
    var hostId = await Sender.Send(command);
    // Assert
    hostId.Value.Should().NotBe(Guid.Empty);
    var createdHost = await Sender.Send(new GetHostQuery(hostId.Value));

    createdHost.Value.Id.Should().Be(hostId.Value);
    createdHost.Value.PersonId.Should().Be(createdHost.Value.PersonId);
  }

  [Fact]
  public async Task CreateHost_WithInvalidRequest_ShouldReturnErrorAsync()
  {
    // Arrange
    var command = new CreateHostCommand(Guid.Empty);
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(PersonErrors.NotFound(Guid.Empty));
  }
}
