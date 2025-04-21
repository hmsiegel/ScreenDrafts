namespace ScreenDrafts.Modules.Drafts.UnitTests.Drafters.Entities;

public class RolloverVetoOverrideTests : BaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenDrafterIsProvided()
  {
    // Arrange
    var drafter = DrafterFactory.CreateDrafter();
    // Act
    var result = RolloverVetoOverride.Create(
      drafter,
      null,
      Guid.NewGuid());
    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenDrafterTeamIsProvided()
  {
    // Arrange
    var drafterTeam = DrafterFactory.CreateDrafterTeam();
    // Act
    var result = RolloverVetoOverride.Create(
      null,
      drafterTeam,
      Guid.NewGuid());
    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public void Create_ShouldReturnFailureResult_WhenBothDrafterAndDrafterTeamAreProvided()
  {
    // Arrange
    var drafter = DrafterFactory.CreateDrafter();
    var drafterTeam = DrafterFactory.CreateDrafterTeam();

    // Act
    var result = RolloverVetoOverride.Create(
      drafter,
      drafterTeam,
      Guid.NewGuid());

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenDrafterIsNullAndDrafterTeamIsNull()
  {
    // Arrange
    Drafter? drafter = null;
    DrafterTeam? drafterTeam = null;

    // Act
    var result = RolloverVetoOverride.Create(
      drafter,
      drafterTeam,
      Guid.NewGuid());

    // Assert
    result.IsFailure.Should().BeTrue();
  }
}
