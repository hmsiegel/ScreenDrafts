using ScreenDrafts.Modules.Drafts.Features.DrafterTeams.AddDrafterToTeam;
using ScreenDrafts.Modules.Drafts.Features.DrafterTeams.Create;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DrafterTeams;

public sealed class AddDrafterToTeamTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task AddDrafterToTeam_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var teamId = await teamFactory.CreateAndSaveTeamAsync();
    var drafterId = await teamFactory.CreateAndSaveDrafterAsync();

    var command = new AddDrafterToTeamCommand
    {
      DrafterTeamId = teamId,
      DrafterId = drafterId
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task AddDrafterToTeam_WithMultipleDrafters_ShouldSucceedAsync()
  {
    // Arrange
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var teamId = await teamFactory.CreateAndSaveTeamAsync();
    var drafterId1 = await teamFactory.CreateAndSaveDrafterAsync();
    var drafterId2 = await teamFactory.CreateAndSaveDrafterAsync();

    await Sender.Send(new AddDrafterToTeamCommand { DrafterTeamId = teamId, DrafterId = drafterId1 }, TestContext.Current.CancellationToken);

    var command = new AddDrafterToTeamCommand
    {
      DrafterTeamId = teamId,
      DrafterId = drafterId2
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task AddDrafterToTeam_WithNonExistentTeam_ShouldFailAsync()
  {
    // Arrange
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var drafterId = await teamFactory.CreateAndSaveDrafterAsync();

    var command = new AddDrafterToTeamCommand
    {
      DrafterTeamId = "nonexistent-team-id",
      DrafterId = drafterId
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task AddDrafterToTeam_WithNonExistentDrafter_ShouldFailAsync()
  {
    // Arrange
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var teamId = await teamFactory.CreateAndSaveTeamAsync();

    var command = new AddDrafterToTeamCommand
    {
      DrafterTeamId = teamId,
      DrafterId = "nonexistent-drafter-id"
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task AddDrafterToTeam_ShouldPersistDrafterInTeamAsync()
  {
    // Arrange
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var teamId = await teamFactory.CreateAndSaveTeamAsync();
    var drafterId = await teamFactory.CreateAndSaveDrafterAsync();

    var command = new AddDrafterToTeamCommand
    {
      DrafterTeamId = teamId,
      DrafterId = drafterId
    };

    // Act
    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    var team = await DbContext.DrafterTeams
      .Include(t => t.Drafters)
      .SingleAsync(t => t.PublicId == teamId, TestContext.Current.CancellationToken);

    team.Drafters.Should().HaveCount(1);
    team.Drafters.Should().Contain(d => d.PublicId == drafterId);
  }
}
