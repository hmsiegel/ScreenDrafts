namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards.AddMovieToDraftBoard;

internal sealed record AddMovieToDraftBoardRequest
{
  [FromRoute(Name = "publicId")]
  public string DraftId { get; init; } = default!;

  public int TmdbId { get; init; } = default!;
  public string? Notes { get; init; } = default!;
  public int? Priority { get; init; } = default!;
}
