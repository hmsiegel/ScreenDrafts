using ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Queries.GetDraftPositionsByGameBoard;

namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Queries.GetGameBoardWithDraftPositions;

public sealed record GetGameBoardWithDraftPositionsResponse(
  Guid Id,
  Guid DraftId,
  IReadOnlyList<DraftPositionResponse> DraftPositions)
{
  public GetGameBoardWithDraftPositionsResponse()
    : this(
        Id: Guid.Empty,
        DraftId: Guid.Empty,
        DraftPositions: [])
  {
  }
}
