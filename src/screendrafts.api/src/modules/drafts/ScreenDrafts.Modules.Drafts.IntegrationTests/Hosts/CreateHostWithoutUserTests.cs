namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Hosts;

public sealed class CreateHostWithoutUserTests(IntegrationTestWebAppFactory factory)
  : BaseIntegrationTest(factory)
{
  [Fact]
  public async Task CreateHost_WithValidRequest_ShouldReturnHostAsync()
  {
    // Arrange
    var host = HostsFactory.CreateHost().Value;
    var command = new CreateHostWithoutUserCommand(
      host.HostName);
    // Act
    var hostId = await Sender.Send(command);
    // Assert
    hostId.Value.Should().NotBe(Guid.Empty);
    var createdHost = await Sender.Send(new GetHostQuery(hostId.Value));

    createdHost.Value.Id.Should().Be(hostId.Value);
    createdHost.Value.Name.Should().Be(host.HostName);
  }

  [Fact]
  public async Task CreateHost_WithInvalidRequest_ShouldReturnErrorAsync()
  {
    // Arrange
    var command = new CreateHostWithoutUserCommand(string.Empty);
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(HostErrors.CannotCreateHost);
  }
}
