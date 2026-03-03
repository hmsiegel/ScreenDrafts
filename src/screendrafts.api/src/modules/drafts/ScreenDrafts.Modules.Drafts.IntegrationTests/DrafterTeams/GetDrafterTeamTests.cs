using ScreenDrafts.Modules.Drafts.Features.DrafterTeams.AddDrafterToTeam;
using ScreenDrafts.Modules.Drafts.Features.DrafterTeams.Get;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DrafterTeams;

public sealed class GetDrafterTeamTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task GetDrafterTeam_WithValidId_ShouldReturnTeamAsync()
  {
    // Arrange
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var teamId = await teamFactory.CreateAndSaveTeamAsync();

    // Act
    var result = await Sender.Send(new GetDrafterTeamQuery { PublicId = teamId });

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.PublicId.Should().Be(teamId);
    result.Value.Name.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task GetDrafterTeam_WithInvalidId_ShouldReturnNotFoundErrorAsync()
  {
    // Arrange
    var nonExistentId = Faker.Random.AlphaNumeric(10);

    // Act
    var result = await Sender.Send(new GetDrafterTeamQuery { PublicId = nonExistentId });

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task GetDrafterTeam_WithNoMembers_ShouldReturnEmptyMembersListAsync()
  {
    // Arrange
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var teamId = await teamFactory.CreateAndSaveTeamAsync();

    // Act
    var result = await Sender.Send(new GetDrafterTeamQuery { PublicId = teamId });

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Members.Should().BeEmpty();
  }

  [Fact]
  public async Task GetDrafterTeam_WithOneMember_ShouldReturnMemberAsync()
  {
    // Arrange
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var teamId = await teamFactory.CreateAndSaveTeamAsync();
    var drafterId = await teamFactory.CreateAndSaveDrafterAsync();

    await Sender.Send(new AddDrafterToTeamCommand { DrafterTeamId = teamId, DrafterId = drafterId });

    // Act
    var result = await Sender.Send(new GetDrafterTeamQuery { PublicId = teamId });

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Members.Should().HaveCount(1);
    result.Value.Members[0].PublicId.Should().Be(drafterId);
    result.Value.Members[0].DisplayName.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task GetDrafterTeam_WithMultipleMembers_ShouldReturnAllMembersAsync()
  {
    // Arrange
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var teamId = await teamFactory.CreateAndSaveTeamAsync();
    var drafterId1 = await teamFactory.CreateAndSaveDrafterAsync();
    var drafterId2 = await teamFactory.CreateAndSaveDrafterAsync();

    await Sender.Send(new AddDrafterToTeamCommand { DrafterTeamId = teamId, DrafterId = drafterId1 });
    await Sender.Send(new AddDrafterToTeamCommand { DrafterTeamId = teamId, DrafterId = drafterId2 });

    // Act
    var result = await Sender.Send(new GetDrafterTeamQuery { PublicId = teamId });

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Members.Should().HaveCount(2);
    result.Value.Members.Should().Contain(m => m.PublicId == drafterId1);
    result.Value.Members.Should().Contain(m => m.PublicId == drafterId2);
  }

  [Fact]
  public async Task GetDrafterTeam_NewlyCreated_ShouldHaveDefaultCapacityAsync()
  {
    // Arrange
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var teamId = await teamFactory.CreateAndSaveTeamAsync();

    // Act
    var result = await Sender.Send(new GetDrafterTeamQuery { PublicId = teamId });

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    // NumberOfDrafters is the team capacity, defaulting to 2
    result.Value.NumberOfDrafters.Should().Be(2);
  }
}
