namespace ScreenDrafts.Modules.Drafts.UnitTests.Drafters.Entities;

public class VetoTests : BaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var pick = PickFactory.CreatePick().Value;
    var drafter = DrafterFactory.CreateDrafter();

    // Act
    var result = Veto.Create(pick, drafter, null);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Pick.Should().Be(pick);
    result.Value.Drafter.Should().Be(drafter);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenPickIsNull()
  {
    // Arrange
    Pick? pick = null;
    var drafter = DrafterFactory.CreateDrafter();
    var drafterTeam = DrafterFactory.CreateDrafterTeam();

    // Act
    var result = Veto.Create(pick!, drafter, drafterTeam);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenDrafterIsNull()
  {
    // Arrange
    var pick = PickFactory.CreatePick().Value;
    Drafter? drafter = null;
    DrafterTeam? drafterTeam = null;

    // Act
    var result = Veto.Create(pick, drafter!, drafterTeam);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenDrafterTeamIsNull()
  {
    // Arrange
    var pick = PickFactory.CreatePick().Value;
    Drafter? drafter = null;
    DrafterTeam? drafterTeam = null;

    // Act
    var result = Veto.Create(pick, drafter, drafterTeam!);

    // Assert
    result.IsFailure.Should().BeTrue();
  }
}
