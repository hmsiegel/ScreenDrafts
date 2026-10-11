namespace ScreenDrafts.Modules.GuestDrafts.Features.Invitations.List;

// ── Query / responses ─────────────────────────────────────────────────────────
internal sealed record ListGuestDraftInvitationsQuery : IQuery<ListGuestDraftInvitationsResponse>
{
  public required string GuestDraftPublicId { get; init; }
  public required string CallerUserPublicId { get; init; }
}
