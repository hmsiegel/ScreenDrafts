using ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Queries.GetDraftPositionsByGameBoard;

namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Queries.GetDraftPosition;

public sealed record GetDraftPositionQuery(Guid GameBoardId, Guid PositionId)
  : IQuery<DraftPositionResponse>;
