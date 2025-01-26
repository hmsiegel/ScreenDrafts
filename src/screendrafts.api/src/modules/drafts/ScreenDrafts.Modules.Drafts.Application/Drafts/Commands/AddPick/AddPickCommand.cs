namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddPick;

public sealed record AddPickCommand(
  Guid DraftId,
  int Position,
  Guid MovieId,
  Guid DrafterId) : ICommand;
