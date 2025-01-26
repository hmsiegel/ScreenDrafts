namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;
public static class GameBoardErrors
{
  public static readonly SDError DraftPositionsMissing =
    SDError.Problem(
    "DraftPositionsMissing",
    "Draft positions are required to create a game board.");
}
