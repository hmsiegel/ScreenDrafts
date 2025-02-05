namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AssignTriviaResults;
public sealed record AssignTriviaResultsCommand(
  Guid DrafterId,
  Guid DraftId,
  int QuestionsWon,
  int Position) : ICommand;
