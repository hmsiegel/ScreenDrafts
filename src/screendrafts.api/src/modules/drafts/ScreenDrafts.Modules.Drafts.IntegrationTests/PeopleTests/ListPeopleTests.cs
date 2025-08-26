namespace ScreenDrafts.Modules.Drafts.IntegrationTests.PeopleTests;
public sealed class ListPeopleTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task Should_ReturnEmptyList_WhenNoPeopleExistAsync()
  {
    // Act
    var result = await Sender.Send(new ListPeopleQuery(Page: 1, PageSize: 20));
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
  }

  [Fact]
  public async Task Should_ReturnPeople_WhenPeopleExistAsync()
  {
    var peopleFactory = new PeopleFactory(Sender, Faker);
    // Arrange
    for (var i = 0; i < 10; i++)
    {
      var person = peopleFactory.CreatePerson();
      await Sender.Send(new CreatePersonCommand(person.FirstName, person.LastName));
    }

    // Act
    var results = await Sender.Send(new ListPeopleQuery(Page: 1, PageSize: 20));

    // Assert
    results.IsSuccess.Should().BeTrue();
    results.Value.Items.Should().HaveCount(10);
  }
}
