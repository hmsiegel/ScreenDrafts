namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraftPosition;

public sealed record GetDraftPositionQuery(Guid GameBoardId, Guid PositionId)
  : IQuery<DraftPositionResponse>;
