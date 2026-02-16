using ScreenDrafts.Modules.Drafts.Features.People.Search;
using ScreenDrafts.Modules.Drafts.IntegrationTests.TestUtils;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.People;

public sealed class SearchPeopleTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task SearchPeople_WhenNoPeopleExist_ShouldReturnEmptyListAsync()
  {
    // Arrange
    var query = new SearchPeopleQuery("John", 10);

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
  }

  [Fact]
  public async Task SearchPeople_WithMatchingName_ShouldReturnMatchingPeopleAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    await peopleFactory.CreateAndSavePersonWithNameAsync("John", "Doe");
    await peopleFactory.CreateAndSavePersonWithNameAsync("John", "Smith");
    await peopleFactory.CreateAndSavePersonWithNameAsync("Jane", "Doe");

    var query = new SearchPeopleQuery("John", 10);

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCountGreaterThanOrEqualTo(2);
    result.Value.Items.Should().Contain(p => p.FirstName == "John");
  }

  [Fact]
  public async Task SearchPeople_WithPartialMatch_ShouldReturnMatchingPeopleAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    await peopleFactory.CreateAndSavePersonWithNameAsync("Johnny", "Depp");
    await peopleFactory.CreateAndSavePersonWithNameAsync("Jonathan", "Smith");
    await peopleFactory.CreateAndSavePersonWithNameAsync("Jane", "Doe");

    var query = new SearchPeopleQuery("John", 10);

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCountGreaterThanOrEqualTo(2);
  }

  [Fact]
  public async Task SearchPeople_WithLimit_ShouldRespectLimitAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    await peopleFactory.CreateAndSavePersonWithNameAsync("John", "Doe");
    await peopleFactory.CreateAndSavePersonWithNameAsync("John", "Smith");
    await peopleFactory.CreateAndSavePersonWithNameAsync("John", "Johnson");
    await peopleFactory.CreateAndSavePersonWithNameAsync("John", "Williams");

    var query = new SearchPeopleQuery("John", 2);

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCountLessThanOrEqualTo(2);
  }

  [Fact]
  public async Task SearchPeople_WithEmptySearch_ShouldReturnErrorOrEmptyAsync()
  {
    // Arrange
    var query = new SearchPeopleQuery(string.Empty, 10);

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task SearchPeople_CaseInsensitive_ShouldReturnMatchingPeopleAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    await peopleFactory.CreateAndSavePersonWithNameAsync("John", "Doe");

    var query = new SearchPeopleQuery("john", 10);

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCountGreaterThanOrEqualTo(1);
  }

  [Fact]
  public async Task SearchPeople_MatchLastName_ShouldReturnMatchingPeopleAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    await peopleFactory.CreateAndSavePersonWithNameAsync("John", "Smith");
    await peopleFactory.CreateAndSavePersonWithNameAsync("Jane", "Smith");
    await peopleFactory.CreateAndSavePersonWithNameAsync("Bob", "Johnson");

    var query = new SearchPeopleQuery("Smith", 10);

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCountGreaterThanOrEqualTo(2);
    result.Value.Items.Should().Contain(p => p.LastName == "Smith");
  }
}
