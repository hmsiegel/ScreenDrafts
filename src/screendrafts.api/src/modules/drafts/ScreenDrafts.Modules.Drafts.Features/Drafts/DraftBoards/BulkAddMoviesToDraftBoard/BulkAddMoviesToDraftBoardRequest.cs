namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards.BulkAddMoviesToDraftBoard;

internal sealed record BulkAddMoviesToDraftBoardRequest
{
  [FromRoute(Name = "draftId")]
  public required string DraftId { get; init; }
  public required IFormFile File { get; init; }
}
