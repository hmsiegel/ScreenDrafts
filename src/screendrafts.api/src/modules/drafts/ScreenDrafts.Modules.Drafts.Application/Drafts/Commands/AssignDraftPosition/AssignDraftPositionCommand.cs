namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AssignDraftPosition;

public sealed record AssignDraftPositionCommand(
  Guid DraftId,
  Guid DrafterId,
  Guid PositionId)
  : ICommand;
