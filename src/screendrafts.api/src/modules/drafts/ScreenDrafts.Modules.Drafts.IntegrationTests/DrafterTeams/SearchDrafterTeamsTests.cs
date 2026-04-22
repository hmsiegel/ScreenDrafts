using ScreenDrafts.Modules.Drafts.Features.DrafterTeams.Create;
using ScreenDrafts.Modules.Drafts.Features.DrafterTeams.Search;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DrafterTeams;

public sealed class SearchDrafterTeamsTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task SearchDrafterTeams_WithNoTeams_ShouldReturnEmptyAsync()
  {
    // Act
    var result = await Sender.Send(new SearchDrafterTeamsQuery(), TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
    result.Value.TotalCount.Should().Be(0);
  }

  [Fact]
  public async Task SearchDrafterTeams_WithNoFilter_ShouldReturnAllTeamsAsync()
  {
    // Arrange
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var id1 = await teamFactory.CreateAndSaveTeamAsync();
    var id2 = await teamFactory.CreateAndSaveTeamAsync();
    var id3 = await teamFactory.CreateAndSaveTeamAsync();

    // Act
    var result = await Sender.Send(new SearchDrafterTeamsQuery { PageSize = 10 }, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TotalCount.Should().Be(3);
    result.Value.Items.Should().HaveCount(3);
    result.Value.Items.Should().Contain(t => t.PublicId == id1);
    result.Value.Items.Should().Contain(t => t.PublicId == id2);
    result.Value.Items.Should().Contain(t => t.PublicId == id3);
  }

  [Fact]
  public async Task SearchDrafterTeams_ByName_ShouldReturnMatchingTeamsAsync()
  {
    // Arrange
    var uniquePrefix = "UniqueAlpha_" + Faker.Random.AlphaNumeric(6);
    await Sender.Send(new CreateDrafterTeamCommand { Name = uniquePrefix + "_Team1" }, TestContext.Current.CancellationToken);
    await Sender.Send(new CreateDrafterTeamCommand { Name = uniquePrefix + "_Team2" }, TestContext.Current.CancellationToken);
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    await teamFactory.CreateAndSaveTeamAsync(); // unrelated team

    // Act
    var result = await Sender.Send(new SearchDrafterTeamsQuery { Name = uniquePrefix }, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TotalCount.Should().Be(2);
    result.Value.Items.Should().HaveCount(2);
    result.Value.Items.Should().AllSatisfy(t => t.Name.Should().Contain(uniquePrefix));
  }

  [Fact]
  public async Task SearchDrafterTeams_ByName_ShouldBeCaseInsensitiveAsync()
  {
    // Arrange
    var baseName = "CaseTest_" + Faker.Random.AlphaNumeric(6);
    await Sender.Send(new CreateDrafterTeamCommand { Name = baseName }, TestContext.Current.CancellationToken);

    // Act - search with different casing
    var result = await Sender.Send(new SearchDrafterTeamsQuery { Name = baseName.ToUpperInvariant() }, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TotalCount.Should().Be(1);
    result.Value.Items[0].Name.Should().Be(baseName);
  }

  [Fact]
  public async Task SearchDrafterTeams_ByName_ShouldMatchPartialNameAsync()
  {
    // Arrange
    var suffix = "PartialMatch_" + Faker.Random.AlphaNumeric(6);
    await Sender.Send(new CreateDrafterTeamCommand { Name = "Prefix_" + suffix }, TestContext.Current.CancellationToken);

    // Act - search with only the suffix
    var result = await Sender.Send(new SearchDrafterTeamsQuery { Name = suffix }, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TotalCount.Should().Be(1);
    result.Value.Items[0].Name.Should().Contain(suffix);
  }

  [Fact]
  public async Task SearchDrafterTeams_ByName_WithNoMatch_ShouldReturnEmptyAsync()
  {
    // Arrange
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    await teamFactory.CreateAndSaveTeamAsync();

    // Act
    var result = await Sender.Send(new SearchDrafterTeamsQuery { Name = "zzz_no_match_" + Faker.Random.AlphaNumeric(12) }, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
    result.Value.TotalCount.Should().Be(0);
  }

  [Fact]
  public async Task SearchDrafterTeams_WithPagination_ShouldReturnFirstPageAsync()
  {
    // Arrange
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    await teamFactory.CreateAndSaveTeamAsync();
    await teamFactory.CreateAndSaveTeamAsync();
    await teamFactory.CreateAndSaveTeamAsync();

    // Act
    var result = await Sender.Send(new SearchDrafterTeamsQuery { Page = 1, PageSize = 2 }, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(2);
    result.Value.TotalCount.Should().Be(3);
    result.Value.Page.Should().Be(1);
    result.Value.TotalPages.Should().Be(2);
    result.Value.HasNextPage.Should().BeTrue();
    result.Value.HasPreviousPage.Should().BeFalse();
  }

  [Fact]
  public async Task SearchDrafterTeams_SecondPage_ShouldReturnRemainingItemsAsync()
  {
    // Arrange
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    await teamFactory.CreateAndSaveTeamAsync();
    await teamFactory.CreateAndSaveTeamAsync();
    await teamFactory.CreateAndSaveTeamAsync();

    // Act
    var result = await Sender.Send(new SearchDrafterTeamsQuery { Page = 2, PageSize = 2 }, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(1);
    result.Value.TotalCount.Should().Be(3);
    result.Value.Page.Should().Be(2);
    result.Value.HasPreviousPage.Should().BeTrue();
    result.Value.HasNextPage.Should().BeFalse();
  }

  [Fact]
  public async Task SearchDrafterTeams_ShouldReturnResultsOrderedByNameAscAsync()
  {
    // Arrange - use a shared prefix so we can isolate ordering
    var prefix = "Order_" + Faker.Random.AlphaNumeric(6) + "_";
    await Sender.Send(new CreateDrafterTeamCommand { Name = prefix + "C" }, TestContext.Current.CancellationToken);
    await Sender.Send(new CreateDrafterTeamCommand { Name = prefix + "A" }, TestContext.Current.CancellationToken);
    await Sender.Send(new CreateDrafterTeamCommand { Name = prefix + "B" }, TestContext.Current.CancellationToken);

    // Act
    var result = await Sender.Send(new SearchDrafterTeamsQuery { Name = prefix }, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(3);
    result.Value.Items[0].Name.Should().EndWith("A");
    result.Value.Items[1].Name.Should().EndWith("B");
    result.Value.Items[2].Name.Should().EndWith("C");
  }

  [Fact]
  public async Task SearchDrafterTeams_ResponseShape_ShouldIncludePublicIdAndNameAsync()
  {
    // Arrange
    var teamName = "ShapeTest_" + Faker.Random.AlphaNumeric(6);
    var createResult = await Sender.Send(new CreateDrafterTeamCommand { Name = teamName }, TestContext.Current.CancellationToken);
    var teamId = createResult.Value;

    // Act
    var result = await Sender.Send(new SearchDrafterTeamsQuery { Name = teamName }, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(1);
    var item = result.Value.Items[0];
    item.PublicId.Should().Be(teamId);
    item.Name.Should().Be(teamName);
    item.NumberOfDrafters.Should().Be(2); // default capacity
  }
}
