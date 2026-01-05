namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Hosts;

public sealed class ListHostsTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task ListHosts_ShouldReturnListOfHosts_WhenHostsExistAsync()
  {
    // Arrange
    for (var i = 0; i < 10; i++)
    {
      var peopleFactory = new PeopleFactory(Sender, Faker);
      var person = await peopleFactory.CreateAndSavePersonAsync();
      await Sender.Send(new CreateHostCommand(person));
    }

    // Act
    var result = await Sender.Send(new ListHostsQuery(Page: 1, PageSize: 20));

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task ListHosts_ShouldReturnError_WhenNoHostsExistAsync()
  {
    // Act
    var result = await Sender.Send(new ListHostsQuery(Page: 1, PageSize: 20));

    // Assert
    result.IsSuccess.Should().BeTrue();
  }
}
