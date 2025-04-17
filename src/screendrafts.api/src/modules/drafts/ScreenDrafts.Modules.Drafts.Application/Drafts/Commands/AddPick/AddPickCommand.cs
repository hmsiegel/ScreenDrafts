namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddPick;

public sealed record AddPickCommand(
  Guid DraftId,
  int Position,
  Guid MovieId,
  int PlayOrder,
  Guid? DrafterId,
  Guid? DrafterTeamId) : ICommand<Guid>;
