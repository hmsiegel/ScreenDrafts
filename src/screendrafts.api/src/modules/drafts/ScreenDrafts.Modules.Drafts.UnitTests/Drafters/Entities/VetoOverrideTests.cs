namespace ScreenDrafts.Modules.Drafts.UnitTests.Drafters.Entities;

public class VetoOverrideTests : BaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var veto = VetoFactory.CreateVeto().Value;
    var drafter = DrafterFactory.CreateDrafter();

    // Act
    var result = VetoOverride.Create(veto, drafter, null);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Veto.Should().Be(veto);
    result.Value.Drafter.Should().Be(drafter);
    result.Value.DrafterTeam.Should().Be(null);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenVetoIsNull()
  {
    // Arrange
    Veto? veto = null;
    var drafter = DrafterFactory.CreateDrafter();
    var drafterTeam = DrafterFactory.CreateDrafterTeam();

    // Act
    var result = VetoOverride.Create(veto!, drafter, drafterTeam);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenDrafterIsNull()
  {
    // Arrange
    var veto = VetoFactory.CreateVeto().Value;
    Drafter? drafter = null;

    // Act
    var result = VetoOverride.Create(veto, drafter!, null);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenDrafterTeamIsNull()
  {
    // Arrange
    var veto = VetoFactory.CreateVeto().Value;
    DrafterTeam? drafterTeam = null;

    // Act
    var result = VetoOverride.Create(veto, null, drafterTeam!);

    // Assert
    result.IsFailure.Should().BeTrue();
  }
}
