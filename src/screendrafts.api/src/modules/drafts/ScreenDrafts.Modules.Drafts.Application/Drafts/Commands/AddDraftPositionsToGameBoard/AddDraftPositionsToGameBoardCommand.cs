namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddDraftPositionsToGameBoard;

public sealed record AddDraftPositionsToGameBoardCommand(
  Guid GameBoardId,
  Collection<DraftPositionRequest> DraftPositionRequests) : ICommand;
