namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.MyDrafts.GetMyDrafts;

// ── Response ──────────────────────────────────────────────────────────────────
internal sealed record GetMyGuestDraftsResponse
{
  public required IReadOnlyList<MyGuestDraftSummary> Upcoming { get; init; }
  public required IReadOnlyList<MyGuestDraftSummary> InProgress { get; init; }
  public required IReadOnlyList<MyGuestDraftSummary> Completed { get; init; }
}
