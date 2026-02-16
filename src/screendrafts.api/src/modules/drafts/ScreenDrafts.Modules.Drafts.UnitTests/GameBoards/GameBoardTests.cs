namespace ScreenDrafts.Modules.Drafts.UnitTests.GameBoards;

public class GameBoardTests : DraftsBaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidDraftPartIsProvided()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft();
    draft.AddPart(1, 1, 7);
    var draftPart = draft.Parts.First();

    // Act
    var result = GameBoard.Create(draftPart);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.DraftPart.Should().Be(draftPart);
    result.Value.DraftPartId.Should().Be(draftPart.Id);
    result.Value.DraftPositions.Should().BeEmpty();
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenDraftPartIsNull()
  {
    // Arrange
    DraftPart? draftPart = null;

    // Act
    var result = GameBoard.Create(draftPart!);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GameBoardErrors.GameBoardCreationFailed);
  }

  [Fact]
  public void AssignDraftPositions_ShouldAddPositionsToList_WhenValidPositionsAreProvided()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft();
    draft.AddPart(1, 1, 7);
    var draftPart = draft.Parts.First();
    var drafter1 = DrafterFactory.CreateDrafter();
    var drafter2 = DrafterFactory.CreateDrafter();
    draftPart.AddParticipant(ParticipantId.From(drafter1.Id));
    draftPart.AddParticipant(ParticipantId.From(drafter2.Id));

    var gameBoard = GameBoard.Create(draftPart).Value;

    var draftPositions = new List<DraftPosition>
    {
        DraftPosition.Create(gameBoard,"Position A", [7, 6, 4, 2], false, false).Value,
        DraftPosition.Create(gameBoard,"Position B", [5, 3, 1], false, false).Value
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
    var draft = DraftFactory.CreateStandardDraft();
    draft.AddPart(1, 1, 7);
    var draftPart = draft.Parts.First();
    var gameBoard = GameBoard.Create(draftPart).Value;
    ICollection<DraftPosition>? draftPositions = null;

    // Act
    var result = gameBoard.AssignDraftPositions(draftPositions!);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GameBoardErrors.DraftPositionsMissing);
  }

  [Fact]
  public void AssignDraftPositions_ShouldReturnFailure_WhenNumberOfPositionsDoesNotMatchParticipants()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft();
    draft.AddPart(1, 1, 7);
    var draftPart = draft.Parts.First();
    var drafter1 = DrafterFactory.CreateDrafter();
    var drafter2 = DrafterFactory.CreateDrafter();
    draftPart.AddParticipant(ParticipantId.From(drafter1.Id));
    draftPart.AddParticipant(ParticipantId.From(drafter2.Id));
    var gameBoard = GameBoard.Create(draftPart).Value;

    var draftPositions = new List<DraftPosition>
    {
        DraftPosition.Create(gameBoard,"Position A", [7, 6, 4, 2], false, false).Value
    };

    // Act
    var result = gameBoard.AssignDraftPositions(draftPositions);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GameBoardErrors.InvalidNumberOfParticipants);
  }

  [Fact]
  public void AssignDraftPositions_ShouldReturnFailure_WhenPositionsListIsEmpty()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft();
    draft.AddPart(1, 1, 7);
    var draftPart = draft.Parts.First();
    var gameBoard = GameBoard.Create(draftPart).Value;

    var draftPositions = new List<DraftPosition>();

    // Act
    var result = gameBoard.AssignDraftPositions(draftPositions);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GameBoardErrors.InvalidNumberOfParticipants);
  }

  [Fact]
  public void Create_WithMultipleParticipants_ShouldCreateSuccessfully()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft();
    draft.AddPart(1, 1, 7);
    var draftPart = draft.Parts.First();
    var drafter1 = DrafterFactory.CreateDrafter();
    var drafter2 = DrafterFactory.CreateDrafter();
    var drafter3 = DrafterFactory.CreateDrafter();
    draftPart.AddParticipant(ParticipantId.From(drafter1.Id));
    draftPart.AddParticipant(ParticipantId.From(drafter2.Id));
    draftPart.AddParticipant(ParticipantId.From(drafter3.Id));
    var gameBoard = GameBoard.Create(draftPart).Value;

    var draftPositions = new List<DraftPosition>
    {
        DraftPosition.Create(gameBoard, "Position A", [9, 6, 3], true, false).Value,
        DraftPosition.Create(gameBoard, "Position B", [8, 5, 2], false, false).Value,
        DraftPosition.Create(gameBoard, "Position C", [7, 4, 1], false, true).Value
    };

    // Act
    var result = GameBoard.Create(draftPart);
    result.Value.AssignDraftPositions(draftPositions);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.DraftPart.Should().Be(draftPart);
    result.Value.DraftPositions.Should().HaveCount(3);
  }
}
