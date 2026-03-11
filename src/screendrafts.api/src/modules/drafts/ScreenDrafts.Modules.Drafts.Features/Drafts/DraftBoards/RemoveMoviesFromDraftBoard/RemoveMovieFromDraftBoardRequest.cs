namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards.RemoveMoviesFromDraftBoard;

internal sealed record RemoveMovieFromDraftBoardRequest
{
  [FromRoute(Name = "publicId")]
  public string DraftId { get; init; } = default!;

  [FromRoute(Name = "tmdbId")]
  public int TmdbId { get; init; } = default!;
  public string? Notes { get; init; } = default!;
  public int? Priority { get; init; } = default!;
}
