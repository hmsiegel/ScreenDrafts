namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetGameBoard;
public sealed record GetGameBoardQuery(Guid DraftId) : IQuery<GameBoardResponse>;
