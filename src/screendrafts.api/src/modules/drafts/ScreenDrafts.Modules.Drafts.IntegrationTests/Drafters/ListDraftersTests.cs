using ScreenDrafts.Modules.Drafts.Features.Drafters.Create;
using ScreenDrafts.Modules.Drafts.Features.Drafters.List;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafters;

public sealed class ListDraftersTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task ListDrafters_WithNoDrafters_ShouldReturnEmptyListAsync()
  {
    // Arrange
    var request = new ListDraftersRequest
    {
      Page = 1,
      PageSize = 10,
      Retired = "all"
    };
    var query = new ListDraftersQuery(request);

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Drafters.Items.Should().BeEmpty();
    result.Value.Drafters.TotalCount.Should().Be(0);
    result.Value.Drafters.Page.Should().Be(1);
    result.Value.Drafters.TotalPages.Should().Be(0);
  }

  [Fact]
  public async Task ListDrafters_WithDrafters_ShouldReturnAllDraftersAsync()
  {
    // Arrange
    var drafterId1 = await CreateDrafterAsync();
    var drafterId2 = await CreateDrafterAsync();
    var drafterId3 = await CreateDrafterAsync();

    var request = new ListDraftersRequest
    {
      Page = 1,
      PageSize = 10,
      Retired = "all"
    };
    var query = new ListDraftersQuery(request);

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Drafters.Items.Should().HaveCount(3);
    result.Value.Drafters.TotalCount.Should().Be(3);
    result.Value.Drafters.Items.Should().Contain(d => d.DrafterId == drafterId1);
    result.Value.Drafters.Items.Should().Contain(d => d.DrafterId == drafterId2);
    result.Value.Drafters.Items.Should().Contain(d => d.DrafterId == drafterId3);
  }

  [Fact]
  public async Task ListDrafters_WithPagination_ShouldReturnCorrectPageAsync()
  {
    // Arrange
    await CreateDrafterAsync();
    await CreateDrafterAsync();
    await CreateDrafterAsync();

    var request = new ListDraftersRequest
    {
      Page = 1,
      PageSize = 2,
      Retired = "all"
    };
    var query = new ListDraftersQuery(request);

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Drafters.Items.Should().HaveCount(2);
    result.Value.Drafters.TotalCount.Should().Be(3);
    result.Value.Drafters.Page.Should().Be(1);
    result.Value.Drafters.TotalPages.Should().Be(2);
    result.Value.Drafters.HasNextPage.Should().BeTrue();
    result.Value.Drafters.HasPreviousPage.Should().BeFalse();
  }

  [Fact]
  public async Task ListDrafters_WithMultipleDrafters_ShouldContainCorrectPropertiesAsync()
  {
    // Arrange
    var personFactory = new PeopleFactory(Sender, Faker);
    var personId = await personFactory.CreateAndSavePersonWithNameAsync("John", "Doe");
    var command = new CreateDrafterCommand(personId);
    var createResult = await Sender.Send(command);
    var drafterId = createResult.Value;

    var request = new ListDraftersRequest
    {
      Page = 1,
      PageSize = 10,
      Retired = "all"
    };
    var query = new ListDraftersQuery(request);

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Drafters.Items.Should().HaveCount(1);
    
    var drafter = result.Value.Drafters.Items[0];
    drafter.DrafterId.Should().Be(drafterId);
    drafter.DisplayName.Should().Contain("John");
    drafter.DisplayName.Should().Contain("Doe");
    drafter.IsRetired.Should().BeFalse();
  }

  private async Task<string> CreateDrafterAsync()
  {
    var personFactory = new PeopleFactory(Sender, Faker);
    var personId = await personFactory.CreateAndSavePersonAsync();
    var command = new CreateDrafterCommand(personId);
    var result = await Sender.Send(command);
    return result.Value;
  }
}
