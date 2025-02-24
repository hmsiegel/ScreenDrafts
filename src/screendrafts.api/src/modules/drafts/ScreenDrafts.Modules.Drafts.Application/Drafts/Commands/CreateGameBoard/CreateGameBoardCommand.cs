namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CreateGameBoard;

public sealed record CreateGameBoardCommand(
    Guid DraftId,
    IEnumerable<DraftPositionDto>? DraftPositions = null) : ICommand<Guid>;
