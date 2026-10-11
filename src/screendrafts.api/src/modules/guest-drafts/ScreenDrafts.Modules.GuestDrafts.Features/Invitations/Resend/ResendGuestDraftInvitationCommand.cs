namespace ScreenDrafts.Modules.GuestDrafts.Features.Invitations.Resend;

// ── Command ───────────────────────────────────────────────────────────────────
internal sealed record ResendGuestDraftInvitationCommand : ICommand
{
  public required string GuestDraftPublicId { get; init; }
  public required string CallerUserPublicId { get; init; }
  public required string InvitationPublicId { get; init; }
}
