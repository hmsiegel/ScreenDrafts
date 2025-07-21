namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafters;

public sealed class ListDraftersTest(IntegrationTestWebAppFactory factory)
  : BaseIntegrationTest(factory)
{
  [Fact]
  public async Task Should_ReturnEmptyList_WhenNoDraftersExistAsync()
  {
    // Act
    var result = await Sender.Send(new ListDraftersQuery(Page: 1, PageSize: 20));
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
  }

  [Fact]
  public async Task Should_ReturnDrafters_WhenDraftersExistAsync()
  {
    // Arrange
    for (var i = 0; i < 10; i++)
    {
      var peopleFactory = new PeopleFactory(Sender, Faker);
      var person = await peopleFactory.CreateAndSavePersonAsync();
      await Sender.Send(new CreateDrafterCommand(person));
    }

    // Act
    var results = await Sender.Send(new ListDraftersQuery(Page: 1, PageSize: 20));

    // Assert
    results.IsSuccess.Should().BeTrue();
    results.Value.Items.Should().HaveCount(10);
  }
}
