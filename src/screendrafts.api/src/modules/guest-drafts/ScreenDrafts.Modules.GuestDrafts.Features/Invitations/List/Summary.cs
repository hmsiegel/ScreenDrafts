using FastEndpoints;

namespace ScreenDrafts.Modules.GuestDrafts.Features.Invitations.List;

// ── Summary ───────────────────────────────────────────────────────────────────
internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "List a guest draft's email invitations";
    Description = "Owner-only. Returns pending and accepted invitations with resend availability.";
    Response<ListGuestDraftInvitationsResponse>(StatusCodes.Status200OK, "Invitations.");
    Response(StatusCodes.Status403Forbidden, "Only the guest draft's owner can list invitations.");
    Response(StatusCodes.Status404NotFound, "Guest draft not found.");
  }
}
