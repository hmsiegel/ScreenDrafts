using ScreenDrafts.Common.Abstractions.Errors;

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

  public static readonly SDError InvalidNumberOfParticipants =
    SDError.Problem(
      "GameBoard.InvalidNumberOfParticipants",
      "The number of participants does not match the number of draft positions.");

  public static SDError DraftPositionAlreadyAssigned(Guid id) =>
    SDError.Conflict(
      "GameBoard.DraftPositionAlreadyAssigned",
      $"The draft position with Id {id} is already assigned to a participant.");

  public static readonly SDError GameBoardCreationFailed =
    SDError.Problem(
      "GameBoard.GameBoardCreationFailed",
      "There was a problem creating the gameboard.");
}
