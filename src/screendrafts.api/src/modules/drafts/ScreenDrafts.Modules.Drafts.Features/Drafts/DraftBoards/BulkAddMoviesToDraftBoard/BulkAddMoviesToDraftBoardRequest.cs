namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards.BulkAddMoviesToDraftBoard;

internal sealed record BulkAddMoviesToDraftBoardRequest
{
  [FromRoute(Name = "draftId")]
  public string DraftId { get; init; } = default!;
  public required IFormFile File { get; init; }
}
