namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.AddDraftPositionsToGameBoard;

public sealed record AddDraftPositionsToGameBoardCommand(
  Guid GameBoardId,
  Collection<DraftPositionRequest> DraftPositionRequests) : ICommand;
