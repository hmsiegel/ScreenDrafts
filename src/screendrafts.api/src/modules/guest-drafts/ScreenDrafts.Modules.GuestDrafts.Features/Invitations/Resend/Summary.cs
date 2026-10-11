using FastEndpoints;

namespace ScreenDrafts.Modules.GuestDrafts.Features.Invitations.Resend;

// ── Summary ───────────────────────────────────────────────────────────────────
internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Resend a guest draft invitation email";
    Description =
      "Owner-only, before the draft starts. Allowed once per 24 hours per invitation, and only while the invitation is pending.";
    Response(StatusCodes.Status204NoContent, "Invitation resent.");
    Response(StatusCodes.Status400BadRequest, "The draft has already started.");
    Response(StatusCodes.Status403Forbidden, "Only the guest draft's owner can resend.");
    Response(StatusCodes.Status404NotFound, "Guest draft or invitation not found.");
    Response(StatusCodes.Status409Conflict, "Already accepted, or resent less than 24 hours ago.");
  }
}
