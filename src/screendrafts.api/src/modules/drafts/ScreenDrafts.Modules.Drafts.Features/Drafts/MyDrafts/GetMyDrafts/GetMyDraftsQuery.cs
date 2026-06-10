namespace ScreenDrafts.Modules.Drafts.Features.Drafts.MyDrafts.GetMyDrafts;

// ── Query ─────────────────────────────────────────────────────────────────────

internal sealed record GetMyDraftsQuery : IQuery<GetMyDraftsResponse>
{
  public required Guid UserId { get; init; }
  public required bool IsAdmin { get; init; }
}
