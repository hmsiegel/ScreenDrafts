using ScreenDrafts.Common.Abstractions.Errors;

namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Errors;

public static class TriviaResultErrors
{
  public static readonly SDError TriviaResultPositionInvalid = SDError.Problem(
    "TriviaResultErrors.PositionInvalid",
    "Trivia position is out of range.");

  public static readonly SDError TriviaResultQuestionsWonInvalid =
    SDError.Problem(
      "TriviaResultErrors.TriviaQuestionsWon",
      "The trivia questions won is out of range.");
}
