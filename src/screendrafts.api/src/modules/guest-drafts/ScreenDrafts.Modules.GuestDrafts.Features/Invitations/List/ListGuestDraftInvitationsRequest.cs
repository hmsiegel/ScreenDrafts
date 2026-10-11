namespace ScreenDrafts.Modules.GuestDrafts.Features.Invitations.List;

// ── Request ───────────────────────────────────────────────────────────────────
internal sealed record ListGuestDraftInvitationsRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;
}
