using ScreenDrafts.Modules.Drafts.Features.People.Search;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.PeopleTests;

public sealed class SearchPeopleTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // ─────────────────────────────────────────────────────────────────────────
  // Empty list
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task SearchPeople_WithNoPeople_ShouldReturnEmptyAsync()
  {
    // Act
    var result = await Sender.Send(new SearchPeopleQuery { Page = 1, PageSize = 10 });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
    result.Value.TotalCount.Should().Be(0);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Basic list
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task SearchPeople_WithNoFilter_ShouldReturnAllPeopleAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    await peopleFactory.CreateAndSavePersonAsync();
    await peopleFactory.CreateAndSavePersonAsync();
    await peopleFactory.CreateAndSavePersonAsync();

    // Act
    var result = await Sender.Send(new SearchPeopleQuery { Page = 1, PageSize = 10 });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(3);
    result.Value.TotalCount.Should().Be(3);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Name filter
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task SearchPeople_ByFirstName_ShouldReturnMatchingPeopleAsync()
  {
    // Arrange
    var uniqueFirst = "UniqueFirst_" + Faker.Random.AlphaNumeric(8);
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var matchId = await peopleFactory.CreateAndSavePersonWithNameAsync(uniqueFirst, "Smith");
    await peopleFactory.CreateAndSavePersonAsync(); // unrelated

    // Act
    var result = await Sender.Send(new SearchPeopleQuery { Name = uniqueFirst });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TotalCount.Should().Be(1);
    result.Value.Items.Single().PublicId.Should().Be(matchId);
  }

  [Fact]
  public async Task SearchPeople_ByLastName_ShouldReturnMatchingPeopleAsync()
  {
    // Arrange
    var uniqueLast = "UniqueLast_" + Faker.Random.AlphaNumeric(8);
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var matchId = await peopleFactory.CreateAndSavePersonWithNameAsync("John", uniqueLast);
    await peopleFactory.CreateAndSavePersonAsync(); // unrelated

    // Act
    var result = await Sender.Send(new SearchPeopleQuery { Name = uniqueLast });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TotalCount.Should().Be(1);
    result.Value.Items.Single().PublicId.Should().Be(matchId);
  }

  [Fact]
  public async Task SearchPeople_ByName_ShouldBeCaseInsensitiveAsync()
  {
    // Arrange
    var firstName = "CaseTest_" + Faker.Random.AlphaNumeric(6);
    var peopleFactory = new PeopleFactory(Sender, Faker);
    await peopleFactory.CreateAndSavePersonWithNameAsync(firstName, "Jones");

    // Act
    var result = await Sender.Send(new SearchPeopleQuery { Name = firstName.ToUpperInvariant() });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TotalCount.Should().Be(1);
    result.Value.Items.Single().FirstName.Should().Be(firstName);
  }

  [Fact]
  public async Task SearchPeople_ByName_ShouldMatchPartiallyAsync()
  {
    // Arrange
    var suffix = "PartialKey_" + Faker.Random.AlphaNumeric(6);
    var peopleFactory = new PeopleFactory(Sender, Faker);
    await peopleFactory.CreateAndSavePersonWithNameAsync("Prefix" + suffix, "Doe");

    // Act
    var result = await Sender.Send(new SearchPeopleQuery { Name = suffix });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TotalCount.Should().Be(1);
    result.Value.Items.Single().FirstName.Should().Contain(suffix);
  }

  [Fact]
  public async Task SearchPeople_ByName_WithNoMatch_ShouldReturnEmptyAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    await peopleFactory.CreateAndSavePersonAsync();

    // Act
    var result = await Sender.Send(new SearchPeopleQuery { Name = "zzz_no_match_" + Faker.Random.AlphaNumeric(12) });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
    result.Value.TotalCount.Should().Be(0);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Role filter
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task SearchPeople_ByRoleDrafter_ShouldReturnOnlyDraftersAsync()
  {
    // Arrange – one drafter, one host, one plain person
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var drafterPersonId = await peopleFactory.CreateAndSavePersonAsync();
    await Sender.Send(new CreateDrafterCommand(drafterPersonId));

    var hostPersonId = await peopleFactory.CreateAndSavePersonAsync();
    await Sender.Send(new CreateHostCommand { PersonPublicId = hostPersonId });

    await peopleFactory.CreateAndSavePersonAsync(); // plain person

    // Act
    var result = await Sender.Send(new SearchPeopleQuery { Role = "drafter", Page = 1, PageSize = 10 });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TotalCount.Should().Be(1);
    result.Value.Items.Single().IsDrafter.Should().BeTrue();
  }

  [Fact]
  public async Task SearchPeople_ByRoleHost_ShouldReturnOnlyHostsAsync()
  {
    // Arrange – one drafter, one host, one plain person
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var drafterPersonId = await peopleFactory.CreateAndSavePersonAsync();
    await Sender.Send(new CreateDrafterCommand(drafterPersonId));

    var hostPersonId = await peopleFactory.CreateAndSavePersonAsync();
    await Sender.Send(new CreateHostCommand { PersonPublicId = hostPersonId });

    await peopleFactory.CreateAndSavePersonAsync(); // plain person

    // Act
    var result = await Sender.Send(new SearchPeopleQuery { Role = "HOST", Page = 1, PageSize = 10 });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TotalCount.Should().Be(1);
    result.Value.Items.Single().IsHost.Should().BeTrue();
  }

  [Fact]
  public async Task SearchPeople_ByRoleDrafter_WithNoDrafters_ShouldReturnEmptyAsync()
  {
    // Arrange – only a plain person, no drafter
    var peopleFactory = new PeopleFactory(Sender, Faker);
    await peopleFactory.CreateAndSavePersonAsync();

    // Act
    var result = await Sender.Send(new SearchPeopleQuery { Role = "drafter" });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Response shape
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task SearchPeople_ResponseShape_ShouldPopulateFieldsAsync()
  {
    // Arrange
    const string firstName = "ShapeFirst";
    const string lastName = "ShapeLast";
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var personId = await peopleFactory.CreateAndSavePersonWithNameAsync(firstName, lastName);
    await Sender.Send(new CreateDrafterCommand(personId));

    // Act
    var result = await Sender.Send(new SearchPeopleQuery { Name = firstName });

    // Assert
    result.IsSuccess.Should().BeTrue();
    var item = result.Value.Items.Single();
    item.PublicId.Should().Be(personId);
    item.FirstName.Should().Be(firstName);
    item.LastName.Should().Be(lastName);
    item.IsDrafter.Should().BeTrue();
    item.IsHost.Should().BeFalse();
  }

  [Fact]
  public async Task SearchPeople_PersonWhoIsBothDrafterAndHost_ShouldShowBothFlagsAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    const string firstName = "BothRoles";
    var personId = await peopleFactory.CreateAndSavePersonWithNameAsync(firstName, "Person");
    await Sender.Send(new CreateDrafterCommand(personId));
    await Sender.Send(new CreateHostCommand { PersonPublicId = personId });

    // Act
    var result = await Sender.Send(new SearchPeopleQuery { Name = firstName });

    // Assert
    result.IsSuccess.Should().BeTrue();
    var item = result.Value.Items.Single();
    item.IsDrafter.Should().BeTrue();
    item.IsHost.Should().BeTrue();
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Pagination
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task SearchPeople_WithPagination_ShouldReturnCorrectPageAsync()
  {
    // Arrange – create 5 people
    var peopleFactory = new PeopleFactory(Sender, Faker);
    for (var i = 0; i < 5; i++)
    {
      await peopleFactory.CreateAndSavePersonAsync();
    }

    // Act
    var result = await Sender.Send(new SearchPeopleQuery { Page = 2, PageSize = 2 });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(2);
    result.Value.TotalCount.Should().Be(5);
    result.Value.Page.Should().Be(2);
    result.Value.TotalPages.Should().Be(3);
    result.Value.HasPreviousPage.Should().BeTrue();
    result.Value.HasNextPage.Should().BeTrue();
  }

  [Fact]
  public async Task SearchPeople_PageSizeCappedAt100Async()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    await peopleFactory.CreateAndSavePersonAsync();

    // Act
    var result = await Sender.Send(new SearchPeopleQuery { Page = 1, PageSize = 500 });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.PageSize.Should().Be(100);
  }
}
