namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CreateGameBoard;

public sealed record CreateGameBoardCommand(
    Guid DraftId,
    string DraftType,
    IEnumerable<DraftPositionDto>? DraftPositions = null) : ICommand;
