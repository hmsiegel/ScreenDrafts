namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards.GetDraftBoard;

internal sealed record GetDraftBoardResponse
{
  public string PublicId { get; init; } = default!;
  public string DraftId { get; init; } = default!;
  public IReadOnlyList<DraftBoardItemResponse> Items { get; init; } = [];
}
