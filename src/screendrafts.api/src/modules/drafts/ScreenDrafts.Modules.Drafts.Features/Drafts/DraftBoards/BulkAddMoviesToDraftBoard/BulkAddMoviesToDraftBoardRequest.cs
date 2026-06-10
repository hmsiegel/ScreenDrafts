namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards.BulkAddMoviesToDraftBoard;

internal sealed record BulkAddMoviesToDraftBoardRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;
  public required IFormFile File { get; init; }
}
