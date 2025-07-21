namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Hosts;

public sealed class GetHostTests(IntegrationTestWebAppFactory factory)
  : BaseIntegrationTest(factory)
{
  [Fact]
  public async Task GetHost_WithValidId_ReturnsHostAsync()
  {
    // Arrange
    var personFactory = new PeopleFactory(Sender, Faker);
    var personId = await personFactory.CreateAndSavePersonAsync();
    var hostId = await Sender.Send(new CreateHostCommand(personId));

    // Act
    var result = await Sender.Send(new GetHostQuery(hostId.Value));

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Id.Should().Be(hostId.Value);
    result.Value.PersonId.Should().Be(result.Value.PersonId);
  }

  [Fact]
  public async Task GetHost_WithInvalidId_ReturnsFailureAsync()
  {
    // Arrange
    var hostId = HostId.Create(Guid.NewGuid());
    // Act
    var result = await Sender.Send(new GetHostQuery(hostId.Value));

      
    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(HostErrors.NotFound(hostId.Value));
  }
}
