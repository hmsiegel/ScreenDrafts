namespace ScreenDrafts.Modules.Drafts.UnitTests.Drafters.Entities;

public class RolloverVetoTests : BaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var drafter = DrafterFactory.CreateDrafter();
    var draftId = DraftId.CreateUnique();

    // Act
    var result = RolloverVeto.Create(drafter, null, draftId.Value);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Drafter.Should().Be(drafter);
    result.Value.FromDraftId.Should().Be(draftId.Value);
  }

  [Fact]
  public void Create_ShouldReturnFailure_DrafterAndTeamAreProvided()
  {
    // Arrange
    var drafter = DrafterFactory.CreateDrafter();
    var drafterTeam = DrafterFactory.CreateDrafterTeam();
    var draftId = DraftId.CreateUnique();
    // Act
    var result = RolloverVeto.Create(drafter, drafterTeam, draftId.Value);
    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenBothDrafterAndTeamAreNull()
  {
    // Arrange
    Drafter? drafter = null;
    DrafterTeam? drafterTeam = null;

    var draftId = DraftId.CreateUnique();
    // Act
    var result = RolloverVeto.Create(drafter, drafterTeam, draftId.Value);
    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenDrafterIsNull()
  {
    // Arrange
    Drafter? drafter = null;
    var draftId = DraftId.CreateUnique();

    // Act
    var result = RolloverVeto.Create(drafter!, null, draftId.Value);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenDraftIdIsEmpty()
  {
    // Arrange
    var drafter = DrafterFactory.CreateDrafter();
    var draftId = Guid.Empty;

    // Act
    var result = RolloverVeto.Create(
      drafter,
      null,
      draftId);

    // Assert
    result.IsFailure.Should().BeTrue();
  }
}
