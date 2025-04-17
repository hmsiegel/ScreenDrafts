namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetGameBoardWithDraftPositions;

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
