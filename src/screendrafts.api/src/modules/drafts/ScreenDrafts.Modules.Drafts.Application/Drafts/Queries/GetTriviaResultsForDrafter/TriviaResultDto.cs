namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetTriviaResultsForDrafter;

public sealed record TriviaResultDto(
  Guid Id,
  int Position,
  int QuestionsWon,
  Guid DraftId,
  Guid DrafterId)
{
  public TriviaResultDto()
    : this(
        Guid.Empty,
        0,
        0,
        Guid.Empty,
        Guid.Empty)
  {
  }
}
