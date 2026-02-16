namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.AddPick;

public sealed record AddPickCommand(
  Guid DraftPartId,
  int Position,
  Guid MovieId,
  int PlayOrder,
  Guid? DrafterId,
  Guid? DrafterTeamId) : ICommand<Guid>;
