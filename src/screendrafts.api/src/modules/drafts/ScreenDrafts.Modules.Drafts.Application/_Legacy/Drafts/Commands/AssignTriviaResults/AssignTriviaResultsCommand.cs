namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.AssignTriviaResults;
public sealed record AssignTriviaResultsCommand(
  Guid DrafterId,
  Guid DraftPartId,
  int QuestionsWon,
  int Position,
  Guid? DrafterTeamId = null) : ICommand;
