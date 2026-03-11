namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards.GetDraftBoard;

internal sealed record GetDraftBoardQuery : IQuery<GetDraftBoardResponse>
{
  public string DraftId { get; init; } = default!;
  public Guid UserId { get; init; }
}
