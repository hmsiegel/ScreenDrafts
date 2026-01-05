namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;

public static partial class DraftErrors
{
  public static readonly SDError CannotAddTriviaResultIfDraftIsNotStarted = SDError.Problem(
    "Drafts.CannotAddTriviaResultIfDraftIsNotStarted",
    "Cannot add a trivia result if the draft is not started.");

  public static readonly SDError CannotAddTriviaResultWithoutDrafterOrDrafterTeam =
    SDError.Problem(
      "Drafts.CannotAddTriviaResultWithoutDrafterOrDrafterTeam",
      "Cannot add a trivia result without a drafter or drafter team.");
}
