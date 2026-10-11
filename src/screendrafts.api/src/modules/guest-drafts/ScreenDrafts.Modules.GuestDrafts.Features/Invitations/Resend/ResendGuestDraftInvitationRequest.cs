namespace ScreenDrafts.Modules.GuestDrafts.Features.Invitations.Resend;

// ── Request ───────────────────────────────────────────────────────────────────
internal sealed record ResendGuestDraftInvitationRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;

  [FromRoute(Name = "invitationPublicId")]
  public string InvitationPublicId { get; init; } = default!;
}
