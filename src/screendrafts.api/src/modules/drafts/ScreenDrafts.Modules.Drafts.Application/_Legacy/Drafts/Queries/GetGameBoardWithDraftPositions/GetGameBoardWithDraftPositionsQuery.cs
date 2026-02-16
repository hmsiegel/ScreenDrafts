namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Queries.GetGameBoardWithDraftPositions;

public sealed record GetGameBoardWithDraftPositionsQuery(Guid DraftId) : IQuery<GetGameBoardWithDraftPositionsResponse>;
