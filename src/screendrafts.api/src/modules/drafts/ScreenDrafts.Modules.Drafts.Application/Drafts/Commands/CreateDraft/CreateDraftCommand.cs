namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CreateDraft;

public sealed record CreateDraftCommand(
  string Title,
  DraftType DraftType,
  int NumberOfDrafters,
  int NumberOfCommissioners,
  int NumberOfMovies) : ICommand<Guid>;
