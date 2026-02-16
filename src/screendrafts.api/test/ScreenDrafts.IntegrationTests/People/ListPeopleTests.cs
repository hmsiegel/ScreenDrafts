using ScreenDrafts.Modules.Drafts.Features.People.List;
using ScreenDrafts.Modules.Drafts.IntegrationTests.TestUtils;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.People;

public sealed class ListPeopleTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task ListPeople_WhenNoPeopleExist_ShouldReturnEmptyListAsync()
  {
    // Arrange
    var request = new ListPeopleRequest { Page = 1, PageSize = 20 };

    // Act
    var result = await Sender.Send(new ListPeopleQuery(request));

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
    result.Value.TotalCount.Should().Be(0);
  }

  [Fact]
  public async Task ListPeople_WhenPeopleExist_ShouldReturnPeopleAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    for (var i = 0; i < 10; i++)
    {
      await peopleFactory.CreateAndSavePersonAsync();
    }

    var request = new ListPeopleRequest { Page = 1, PageSize = 20 };

    // Act
    var result = await Sender.Send(new ListPeopleQuery(request));

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(10);
    result.Value.TotalCount.Should().Be(10);
  }

  [Fact]
  public async Task ListPeople_WithPagination_ShouldReturnCorrectPageAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    for (var i = 0; i < 15; i++)
    {
      await peopleFactory.CreateAndSavePersonAsync();
    }

    var request = new ListPeopleRequest { Page = 2, PageSize = 10 };

    // Act
    var result = await Sender.Send(new ListPeopleQuery(request));

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(5);
    result.Value.TotalCount.Should().Be(15);
  }

  [Fact]
  public async Task ListPeople_WithNameFilter_ShouldReturnFilteredPeopleAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    await peopleFactory.CreateAndSavePersonWithNameAsync("John", "Doe");
    await peopleFactory.CreateAndSavePersonWithNameAsync("Jane", "Smith");
    await peopleFactory.CreateAndSavePersonWithNameAsync("Bob", "Johnson");

    var request = new ListPeopleRequest { Page = 1, PageSize = 20, Name = "John" };

    // Act
    var result = await Sender.Send(new ListPeopleQuery(request));

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCountGreaterThanOrEqualTo(1);
  }

  [Fact]
  public async Task ListPeople_WithCustomPageSize_ShouldRespectPageSizeAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    for (var i = 0; i < 20; i++)
    {
      await peopleFactory.CreateAndSavePersonAsync();
    }

    var request = new ListPeopleRequest { Page = 1, PageSize = 5 };

    // Act
    var result = await Sender.Send(new ListPeopleQuery(request));

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(5);
    result.Value.TotalCount.Should().Be(20);
  }
}
