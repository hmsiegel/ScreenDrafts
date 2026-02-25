using ScreenDrafts.Modules.Drafts.Features.DrafterTeams.AddDrafterToTeam;
using ScreenDrafts.Modules.Drafts.Features.DrafterTeams.RemoveDrafterFromTeam;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DrafterTeams;

public sealed class RemoveDrafterFromTeamTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task RemoveDrafterFromTeam_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var teamId = await teamFactory.CreateAndSaveTeamAsync();
    var drafterId1 = await teamFactory.CreateAndSaveDrafterAsync();
    var drafterId2 = await teamFactory.CreateAndSaveDrafterAsync();

    await Sender.Send(new AddDrafterToTeamCommand { DrafterTeamId = teamId, DrafterId = drafterId1 });
    await Sender.Send(new AddDrafterToTeamCommand { DrafterTeamId = teamId, DrafterId = drafterId2 });

    var command = new RemoveDrafterFromTeamCommand
    {
      DrafterTeamId = teamId,
      DrafterId = drafterId1
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task RemoveDrafterFromTeam_WithNonExistentTeam_ShouldFailAsync()
  {
    // Arrange
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var drafterId = await teamFactory.CreateAndSaveDrafterAsync();

    var command = new RemoveDrafterFromTeamCommand
    {
      DrafterTeamId = "nonexistent-team-id",
      DrafterId = drafterId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task RemoveDrafterFromTeam_WithNonExistentDrafter_ShouldFailAsync()
  {
    // Arrange
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var teamId = await teamFactory.CreateAndSaveTeamAsync();

    var command = new RemoveDrafterFromTeamCommand
    {
      DrafterTeamId = teamId,
      DrafterId = "nonexistent-drafter-id"
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task RemoveDrafterFromTeam_ShouldRemoveDrafterFromPersistenceAsync()
  {
    // Arrange
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var teamId = await teamFactory.CreateAndSaveTeamAsync();
    var drafterId1 = await teamFactory.CreateAndSaveDrafterAsync();
    var drafterId2 = await teamFactory.CreateAndSaveDrafterAsync();

    await Sender.Send(new AddDrafterToTeamCommand { DrafterTeamId = teamId, DrafterId = drafterId1 });
    await Sender.Send(new AddDrafterToTeamCommand { DrafterTeamId = teamId, DrafterId = drafterId2 });

    var command = new RemoveDrafterFromTeamCommand
    {
      DrafterTeamId = teamId,
      DrafterId = drafterId1
    };

    // Act
    await Sender.Send(command);

    // Assert
    var team = await DbContext.DrafterTeams
      .Include(t => t.Drafters)
      .SingleAsync(t => t.PublicId == teamId);

    team.Drafters.Should().HaveCount(1);
    team.Drafters.Should().NotContain(d => d.PublicId == drafterId1);
    team.Drafters.Should().Contain(d => d.PublicId == drafterId2);
  }
}
