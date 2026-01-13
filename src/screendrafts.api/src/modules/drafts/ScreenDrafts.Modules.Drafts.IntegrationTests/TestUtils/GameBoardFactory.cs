namespace ScreenDrafts.Modules.Drafts.IntegrationTests.TestUtils;

public static class GameBoardFactory
{
  public static Result<GameBoard> CreateStandardGameBoard()
  {
    var draft = DraftFactory.CreateStandardDraft().Value;
    Collection<DraftPosition> draftPositions = [];

    for (int i = 0; i < draft.TotalParticipants; i++)
    {
      var draftPosition = DraftPositionFactory.CreateDraftPositions(
        draft.TotalParticipants,
        draft.TotalPicks)[i];

      draftPositions.Add(draftPosition);
    }

    var gameBoard = GameBoard.Create(
      draft);
    gameBoard.Value.AssignDraftPositions(draftPositions);
    return gameBoard;
  }

  public static Result<GameBoard> CreateMiniMegaGameBoard()
  {
    var draft = DraftFactory.CreateMiniMegaDraft().Value;
    Collection<DraftPosition> draftPositions = [];

    for (int i = 0; i < draft.TotalParticipants; i++)
    {
      var draftPosition = DraftPositionFactory.CreateDraftPositions(
        draft.TotalParticipants,
        draft.TotalPicks)[i];

      draftPositions.Add(draftPosition);
    }

    var gameBoard = GameBoard.Create(
      draft);
    gameBoard.Value.AssignDraftPositions(draftPositions);
    return gameBoard;
  }

  public static Result<GameBoard> CreateMegaGameBoard()
  {
    var draft = DraftFactory.CreateMegaDraft().Value;
    Collection<DraftPosition> draftPositions = [];

    for (int i = 0; i < draft.TotalParticipants; i++)
    {
      var draftPosition = DraftPositionFactory.CreateDraftPositions(
        draft.TotalParticipants,
        draft.TotalPicks)[i];

      draftPositions.Add(draftPosition);
    }

    var gameBoard = GameBoard.Create(
      draft);
    gameBoard.Value.AssignDraftPositions(draftPositions);
    return gameBoard;
  }
}
