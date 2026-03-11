namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards.GetDraftBoard;

public sealed record DraftBoardItemResponse
{
  public int TmdbId { get; init; }
  public string? Notes { get; init; }
  public int? Priority { get; init; }
}
