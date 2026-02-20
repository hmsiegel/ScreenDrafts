using ScreenDrafts.Common.Presentation.Responses;
using ScreenDrafts.Modules.Drafts.Features.People;
using ScreenDrafts.Modules.Drafts.Features.People.Create;
using ScreenDrafts.Modules.Drafts.Features.People.List;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.PeopleTests;
public sealed class ListPeopleTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task Should_ReturnEmptyList_WhenNoPeopleExistAsync()
  {
    // Act
    var request = new ListPeopleRequest{Page= 1, PageSize= 20};
    var query = new ListPeopleQuery(request);
    var result = await Sender.Send(query);
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.People.Should().NotBeNull();
  }

  [Fact]
  public async Task Should_ReturnPeople_WhenPeopleExistAsync()
  {
    var peopleFactory = new PeopleFactory(Sender, Faker);
    // Arrange
    for (var i = 0; i < 10; i++)
    {
      var person = peopleFactory.CreatePerson();
      await Sender.Send(new CreatePersonCommand
      {
        FirstName = person.FirstName,
        LastName = person.LastName
      });
    }

    // Act
    var request = new ListPeopleRequest{Page= 1, PageSize= 20};
    var query = new ListPeopleQuery(request);
    var results = await Sender.Send(query);

    // Assert
    results.IsSuccess.Should().BeTrue();
    results.Value.People.Should().BeOfType<PagedResult<PersonResponse>>();
    results.Value.People.Items.Should().HaveCount(10);
    results.Value.People.TotalCount.Should().Be(10);
  }
}
