namespace ScreenDrafts.Modules.Drafts.Features.Drafts.MyDrafts.GetMyDrafts;

// ── Response ──────────────────────────────────────────────────────────────────

internal sealed record GetMyDraftsResponse
{
  public IReadOnlyList<MyDraftSummary> Upcoming { get; init; } = [];
  public IReadOnlyList<MyDraftSummary> InProgress { get; init; } = [];
  public IReadOnlyList<MyDraftSummary> Completed { get; init; } = [];
}
