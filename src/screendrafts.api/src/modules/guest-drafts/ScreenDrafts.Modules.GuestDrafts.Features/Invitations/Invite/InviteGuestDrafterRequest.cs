namespace ScreenDrafts.Modules.GuestDrafts.Features.Invitations.Invite;

// ── Request ───────────────────────────────────────────────────────────────────
internal sealed record InviteGuestDrafterRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;

  public required string Email { get; init; }

  public required string Name { get; init; }
}
