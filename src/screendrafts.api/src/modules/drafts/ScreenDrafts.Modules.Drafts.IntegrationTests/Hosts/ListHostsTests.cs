namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Hosts;

public sealed class ListHostsTests(IntegrationTestWebAppFactory factory) 
  : BaseIntegrationTest(factory)
{
  [Fact]
  public async Task ListHosts_ShouldReturnListOfHosts_WhenHostsExistAsync()
  {
    // Arrange
    List<Host> hosts = [];

    do
    {
      var host = HostsFactory.CreateHost().Value;

      var hostId = await Sender.Send(new CreateHostWithoutUserCommand(host.HostName));

      var createdHost = await Sender.Send(new GetHostQuery(hostId.Value));

      hosts.Add(Host.Create(
        hostName: createdHost.Value.Name,
        id: HostId.Create(createdHost.Value.Id)).Value);

    } while (hosts.Count < 20);

    // Act
    var result = await Sender.Send(new ListHostsQuery());

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().HaveCount(20);
  }

  [Fact]
  public async Task ListHosts_ShouldReturnError_WhenNoHostsExistAsync()
  {
    // Act
    var result = await Sender.Send(new ListHostsQuery());

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().BeEmpty();
  }
}
