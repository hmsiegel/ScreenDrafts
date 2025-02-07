namespace ScreenDrafts.Modules.Drafts.UnitTests.Drafts;
public class GameBoardTests : BaseTest
{
  [Fact]
  public void CreateStandardDraft_ShouldReturnSuccessResult_WhenDraftAndDraftPositionsAreValid()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;

    var draftPositions = DraftFactory.CreateStandardDraftPositions();

    // Act
    var result = GameBoard.Create(
      draft,
      new Collection<DraftPosition>(draftPositions));

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

    var draftPositions = DraftFactory.CreateMegaDraftPositions();

    // Act
    var result = GameBoard.Create(
      draft,
      new Collection<DraftPosition>(draftPositions));

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Draft.Should().Be(draft);
    result.Value.DraftPositions.Should().BeEquivalentTo(draftPositions);
  }

  [Fact]
  public void CreateStandardDraft_ShouldThrowArgumentNullException_WhenDraftPositionsIsNull()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft();

    // Act
    Action act = () => GameBoard.Create(draft.Value, new Collection<DraftPosition>(null!));

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }
}
