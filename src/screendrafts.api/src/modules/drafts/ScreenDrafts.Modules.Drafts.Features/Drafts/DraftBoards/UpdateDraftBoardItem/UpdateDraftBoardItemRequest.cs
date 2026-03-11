namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards.UpdateDraftBoardItem;

internal sealed record UpdateDraftBoardItemRequest
{
  [FromRoute(Name = "publicId")]
  public string DraftId { get; init; } = default!;

  [FromRoute(Name = "tmdbId")]
  public int TmdbId { get; init; } = default!;
  public string? Notes { get; init; } = default!;
  public int? Priority { get; init; } = default!;
}
