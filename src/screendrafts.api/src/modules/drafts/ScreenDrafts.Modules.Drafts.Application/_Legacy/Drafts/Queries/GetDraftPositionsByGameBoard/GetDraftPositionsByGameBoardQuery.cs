namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Queries.GetDraftPositionsByGameBoard;

public sealed record GetDraftPositionsByGameBoardQuery(Guid GameBoardId) : IQuery<IReadOnlyCollection<DraftPositionResponse>>;
