namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.AssignDraftPosition;

public sealed record AssignDraftPositionCommand(
  Guid DraftId,
  Guid DrafterId,
  Guid PositionId)
  : ICommand;
