namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetGameBoardWithDraftPositions;

public sealed record GetGameBoardWithDraftPositionsQuery(Guid DraftId) : IQuery<GetGameBoardWithDraftPositionsResponse>;
