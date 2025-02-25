namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetGameBoard;

public sealed record GameBoardResponse(
  Guid Id,
  Guid DraftId,
  IReadOnlyList<DraftPositionResponse> DraftPositions)
{
  public GameBoardResponse()
    : this(
        Id: Guid.Empty,
        DraftId: Guid.Empty,
        DraftPositions: [])
  {
  }
}
