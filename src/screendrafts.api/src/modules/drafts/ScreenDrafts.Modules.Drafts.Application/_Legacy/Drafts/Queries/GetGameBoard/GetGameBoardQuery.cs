namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Queries.GetGameBoard;
public sealed record GetGameBoardQuery(
  Guid DraftId) : IQuery<GameBoardResponse>;
