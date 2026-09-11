namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Get;

/// <summary>
/// The standalone "view a finished draft" shape — deliberately separate from
/// GuestDraftSummaryResponse (the list-row shape used by SearchDraftsQuery)
/// and everything under GuestDraftGameplay* (the live-session shape, which
/// carries CallerContext, veto token counts, reveal-authorization state, and
/// full veto history that only matter while a draft is actually being
/// played). None of that applies here — a completed draft has nothing left
/// to authorize, veto, or gate by caller identity beyond "were you involved
/// in this draft at all."
/// </summary>
internal sealed record GuestDraftDetailResponse
{
  public required string PublicId { get; init; }
  public required string Title { get; init; }
  public required string Type { get; init; }
  public required string Status { get; init; }
  public DateOnly? DraftDate { get; init; }
  public IReadOnlyList<GuestDraftDetailPositionResponse> Positions { get; init; } = [];
  public IReadOnlyList<GuestDraftDetailPickResponse> Picks { get; init; } = [];
}
