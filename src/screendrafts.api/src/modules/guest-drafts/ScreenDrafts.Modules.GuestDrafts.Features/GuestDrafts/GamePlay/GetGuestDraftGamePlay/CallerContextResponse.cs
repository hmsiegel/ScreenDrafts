namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.GamePlay.GetGuestDraftGamePlay;

// ── Response ──────────────────────────────────────────────────────────────

internal sealed record CallerContextResponse
{
  public bool IsOwner { get; init; }
  public bool IsParticipant { get; init; }
  public string? ParticipantPublicId { get; init; }
}
