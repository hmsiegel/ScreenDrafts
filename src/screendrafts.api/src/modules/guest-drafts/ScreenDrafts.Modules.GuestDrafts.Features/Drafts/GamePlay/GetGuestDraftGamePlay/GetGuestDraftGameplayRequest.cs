namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.GamePlay.GetGuestDraftGamePlay;

// ── Request ───────────────────────────────────────────────────────────────

internal sealed record GetGuestDraftGameplayRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;
}
