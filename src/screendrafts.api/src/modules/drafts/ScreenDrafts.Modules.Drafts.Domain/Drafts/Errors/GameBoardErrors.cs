namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;
public static class GameBoardErrors
{
  public static readonly SDError DraftPositionsMissing =
    SDError.Problem(
    "GameBoard.DraftPositionsMissing",
    "Draft positions are required to create a game board.");

  public static SDError GameBoardNotFound(Guid id) =>
    SDError.NotFound(
      "GameBoard.NotFound",
      $"The game board with Id {id} was not found.");

  public static SDError DraftPositionNotFound(Guid id) =>
    SDError.NotFound(
      "GameBoard.DraftPositionNotFound",
      $"The draft position with Id {id} was not found.");
}
