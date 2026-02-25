using ScreenDrafts.Modules.Drafts.Features.DrafterTeams.Create;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DrafterTeams;

public sealed class CreateDrafterTeamTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task CreateDrafterTeam_WithValidName_ShouldReturnPublicIdAsync()
  {
    // Arrange
    var command = new CreateDrafterTeamCommand
    {
      Name = Faker.Internet.UserName()
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task CreateDrafterTeam_WithDuplicateName_ShouldFailAsync()
  {
    // Arrange
    var name = Faker.Internet.UserName();

    await Sender.Send(new CreateDrafterTeamCommand { Name = name });

    var command = new CreateDrafterTeamCommand { Name = name };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task CreateDrafterTeam_WithEmptyName_ShouldFailAsync()
  {
    // Arrange
    var command = new CreateDrafterTeamCommand
    {
      Name = string.Empty
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task CreateDrafterTeam_WithWhitespaceName_ShouldFailAsync()
  {
    // Arrange
    var command = new CreateDrafterTeamCommand
    {
      Name = "   "
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
  }
}
