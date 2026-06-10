namespace ScreenDrafts.Modules.Drafts.Features.Drafts.MyDrafts.GetMyDraftDetail;

// ── Query ─────────────────────────────────────────────────────────────────────

internal sealed record GetMyDraftDetailQuery : IQuery<GetMyDraftDetailResponse>
{
  public required string DraftId { get; init; }
  public required Guid UserId { get; init; }
  public required bool IsAdmin { get; init; }
}
