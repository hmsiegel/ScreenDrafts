namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.CreateGameBoard;

public sealed record CreateGameBoardCommand(
    Guid DraftId, Guid DraftPartId) : ICommand<Guid>;
