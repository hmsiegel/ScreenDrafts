namespace ScreenDrafts.Modules.Drafts.UnitTests.DrafterTeams;

public class DrafterTeamTests : DraftsBaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var name = "Test Team";

    // Act
    var result = DrafterTeam.Create(name);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Name.Should().Be(name);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenNameIsEmpty()
  {
    // Arrange
    var name = string.Empty;

    // Act
    var result = DrafterTeam.Create(name);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void AddDrafter_ShouldAddDrafterToList_WhenDrafterIsValid()
  {
    // Arrange
    var team = DrafterFactory.CreateDrafterTeam();
    var newDrafter = DrafterFactory.CreateDrafter();

    // Act
    var result = team.AddDrafter(newDrafter);

    // Assert
    result.IsSuccess.Should().BeTrue();
    team.Drafters.Should().Contain(newDrafter);
  }

  [Fact]
  public void AddDrafter_ShouldReturnFailure_WhenDrafterAlreadyExists()
  {
    // Arrange
    var team = DrafterFactory.CreateDrafterTeam();
    var drafter = DrafterFactory.CreateDrafter();
    team.AddDrafter(drafter);

    // Act
    var result = team.AddDrafter(drafter);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void RemoveDrafter_ShouldRemoveDrafterFromList_WhenDrafterExists()
  {
    // Arrange
    var team = DrafterFactory.CreateDrafterTeam();
    var drafter = DrafterFactory.CreateDrafter();
    var drafterToRemove = DrafterFactory.CreateDrafter();
    team.AddDrafter(drafter);
    team.AddDrafter(drafterToRemove);


    // Act
    var result = team.RemoveDrafter(drafterToRemove);

    // Assert
    result.IsSuccess.Should().BeTrue();
    team.Drafters.Should().NotContain(drafterToRemove);
  }

  [Fact]
  public void RemoveDrafter_ShouldReturnFailure_WhenDrafterDoesNotExist()
  {
    // Arrange
    var team = DrafterFactory.CreateDrafterTeam();
    var drafter = DrafterFactory.CreateDrafter();
    team.AddDrafter(drafter);

    var nonExistentDrafter = DrafterFactory.CreateDrafter();

    // Act
    var result = team.RemoveDrafter(nonExistentDrafter);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void RemoveDrafter_ShouldReturnFailure_WhenRemovingLastDrafter()
  {
    // Arrange
    var team = DrafterFactory.CreateDrafterTeam();
    var drafter = DrafterFactory.CreateDrafter();
    var lastDrafter = DrafterFactory.CreateDrafter();

    team.AddDrafter(drafter);
    team.AddDrafter(lastDrafter);
    team.RemoveDrafter(drafter);

    // Act
    var result = team.RemoveDrafter(lastDrafter);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void SetTeamName_ShouldUpdateName_WhenValidNameIsProvided()
  {
    // Arrange
    var team = DrafterFactory.CreateDrafterTeam();
    var newName = "New Team Name";

    // Act
    team.UpdateName(newName);

    // Assert
    team.Name.Should().Be(newName);
  }

  [Fact]
  public void SetTeamName_ShouldReturnFailure_WhenNameIsEmpty()
  {
    // Arrange
    var team = DrafterFactory.CreateDrafterTeam();
    var emptyName = string.Empty;

    // Act
    var result = team.UpdateName(emptyName);

    // Assert
    result.IsFailure.Should().BeTrue();
  }
}
