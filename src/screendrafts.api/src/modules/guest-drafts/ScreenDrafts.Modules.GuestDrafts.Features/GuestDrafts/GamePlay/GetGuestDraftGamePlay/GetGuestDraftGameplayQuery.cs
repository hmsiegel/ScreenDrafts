namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.GamePlay.GetGuestDraftGamePlay;

// ── Query ─────────────────────────────────────────────────────────────────

internal sealed record GetGuestDraftGameplayQuery : IQuery<GetGuestDraftGameplayResponse>
{
  public required string GuestDraftPublicId { get; init; }
  public required string CallerUserPublicId { get; init; }
}
