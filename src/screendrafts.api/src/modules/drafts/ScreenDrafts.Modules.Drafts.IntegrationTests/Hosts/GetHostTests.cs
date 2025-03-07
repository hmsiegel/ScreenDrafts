namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Hosts;

public sealed class GetHostTests(IntegrationTestWebAppFactory factory)
  : BaseIntegrationTest(factory)
{
  [Fact]
  public async Task GetHost_WithValidId_ReturnsHostAsync()
  {
    // Arrange
    var host = HostsFactory.CreateHost().Value;
    var hostId = await Sender.Send(new CreateHostWithoutUserCommand(host.HostName));

    // Act
    var result = await Sender.Send(new GetHostQuery(hostId.Value));

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Id.Should().Be(hostId.Value);
    result.Value.Name.Should().Be(host.HostName);
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
