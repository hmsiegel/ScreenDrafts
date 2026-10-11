using FastEndpoints;

namespace ScreenDrafts.Modules.GuestDrafts.Features.Invitations.Invite;

// ── Summary ───────────────────────────────────────────────────────────────────
internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Invite an unregistered person to a guest draft by email";
    Description =
      "Owner-only, before the draft starts. Stores a pending invitation and emails a sign-up link. The invitee joins the draft automatically when they register with that email. Rejects emails that already belong to a registered user, duplicates, and more than 10 invitations per draft.";
    Response<InviteGuestDrafterResponse>(StatusCodes.Status200OK, "Invitation created.");
    Response(StatusCodes.Status400BadRequest, "Invalid email or name, or the draft has started.");
    Response(StatusCodes.Status403Forbidden, "Only the guest draft's owner can invite.");
    Response(StatusCodes.Status404NotFound, "Guest draft not found.");
    Response(
      StatusCodes.Status409Conflict,
      "Email already registered, already invited, or invitation cap reached."
    );
  }
}
