namespace ScreenDrafts.Modules.Drafts.UnitTests.Drafts.Entities;

public class GameBoardTests : BaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidDraftIsProvided()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;

    // Act
    var result = GameBoard.Create(draft);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Draft.Should().Be(draft);
    result.Value.DraftId.Should().Be(draft.Id);
    result.Value.DraftPositions.Should().BeEmpty();
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenDraftIsNull()
  {
    // Arrange
    Draft? draft = null;

    // Act
    var result = GameBoard.Create(draft!);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void AssignDraftPositions_ShouldAddPositionsToList_WhenValidPositionsAreProvided()
  {
    // Arrange
    var draft = DraftTests.SetupAndStartDraft();

    var gameBoard = GameBoard.Create(draft).Value;

    var draftPositions = new List<DraftPosition>
        {
            DraftPosition.Create("Drafter A", [7, 6, 4, 2]).Value,
            DraftPosition.Create("Drafter B", [5, 3, 1]).Value
        };

    // Act
    var result = gameBoard.AssignDraftPositions(draftPositions);

    // Assert
    result.IsSuccess.Should().BeTrue();
    gameBoard.DraftPositions.Should().BeEquivalentTo(draftPositions);
  }

  [Fact]
  public void AssignDraftPositions_ShouldReturnFailure_WhenPositionsAreNull()
  {
    // Arrange
    var gameBoard = DraftFactory.CreateStandardGameBoard().Value;
    ICollection<DraftPosition>? draftPositions = null;

    // Act
    var result = gameBoard.AssignDraftPositions(draftPositions!);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(GameBoardErrors.DraftPositionsMissing);
  }

  [Fact]
  public void AssignDraftPositions_ShouldReturnFailure_WhenNumberOfDraftersDoesNotMatch()
  {
    // Arrange
    var gameBoard = DraftFactory.CreateStandardGameBoard().Value;

    var draftPositions = new List<DraftPosition>
        {
            DraftPosition.Create("Drafter A", [7, 6, 4, 2]).Value
        };

    // Act
    var result = gameBoard.AssignDraftPositions(draftPositions);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(GameBoardErrors.InvalidNumberOfDrafters);
  }

  [Fact]
  public void CreateStandardDraft_ShouldReturnSuccessResult_WhenDraftAndDraftPositionsAreValid()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;

    for (int i = 0; i < draft.TotalParticipants; i++)
    {
      draft.AddDrafter(DrafterFactory.CreateDrafter());
    }

    var draftPositions = DraftFactory.CreateStandardDraftPositions();

    // Act
    var result = GameBoard.Create(
      draft);

    result.Value.AssignDraftPositions(draftPositions);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Draft.Should().Be(draft);
    result.Value.DraftPositions.Should().BeEquivalentTo(draftPositions);
  }

  [Fact]
  public void CreateMegaDraft_ShouldReturnSuccessResult_WhenDraftAndDraftPositionsAreValid()
  {
    // Arrange
    var draft = DraftFactory.CreateMegaDraft().Value;

    for (int i = 0; i < draft.TotalParticipants; i++)
    {
      draft.AddDrafter(DrafterFactory.CreateDrafter());
    }

    var draftPositions = DraftFactory.CreateMegaDraftPositions();

    // Act
    var result = GameBoard.Create(
      draft);
    result.Value.AssignDraftPositions(draftPositions);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Draft.Should().Be(draft);
    result.Value.DraftPositions.Should().BeEquivalentTo(draftPositions);
  }
}
