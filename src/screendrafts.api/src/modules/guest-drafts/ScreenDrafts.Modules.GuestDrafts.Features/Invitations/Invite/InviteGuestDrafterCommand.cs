namespace ScreenDrafts.Modules.GuestDrafts.Features.Invitations.Invite;

// ── Command / response ────────────────────────────────────────────────────────
internal sealed record InviteGuestDrafterCommand : ICommand<InviteGuestDrafterResponse>
{
  public required string GuestDraftPublicId { get; init; }
  public required string CallerUserPublicId { get; init; }
  public required string Email { get; init; }
  public required string Name { get; init; }
}
