namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.GamePlay.GetGuestDraftGamePlay;

// ── Query ─────────────────────────────────────────────────────────────────

internal sealed record GetDraftGameplayQuery : IQuery<GetGuestDraftGameplayResponse>
{
  public required string GuestDraftPublicId { get; init; }
  public required string CallerUserPublicId { get; init; }
}
