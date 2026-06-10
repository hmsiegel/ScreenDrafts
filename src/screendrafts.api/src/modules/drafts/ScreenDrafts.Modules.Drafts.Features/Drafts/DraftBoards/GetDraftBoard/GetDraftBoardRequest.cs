namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards.GetDraftBoard;

internal sealed record GetDraftBoardRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;
}
