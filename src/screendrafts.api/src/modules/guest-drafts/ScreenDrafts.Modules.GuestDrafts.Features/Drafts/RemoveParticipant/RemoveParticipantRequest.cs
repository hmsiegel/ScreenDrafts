namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.RemoveParticipant;

// ── Request ───────────────────────────────────────────────────────────────────
internal sealed record RemoveParticipantRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;

  [FromRoute(Name = "guestDrafterPublicId")]
  public string GuestDrafterPublicId { get; init; } = default!;
}
