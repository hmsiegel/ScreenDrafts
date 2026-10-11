namespace ScreenDrafts.Modules.GuestDrafts.Features.Invitations.List;

internal sealed record ListGuestDraftInvitationsResponse
{
  public IReadOnlyList<GuestDraftInvitationResponse> Invitations { get; init; } = [];
}
