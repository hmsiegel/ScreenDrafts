using FastEndpoints;

namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.RemoveParticipant;

// ── Summary ───────────────────────────────────────────────────────────────────
internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Remove a participant from a guest draft";
    Description =
      "Owner-only, before the draft starts. Removes the participant and frees any board position assigned to them.";
    Response(StatusCodes.Status204NoContent, "Participant removed.");
    Response(StatusCodes.Status400BadRequest, "The draft has already started.");
    Response(
      StatusCodes.Status403Forbidden,
      "Only the guest draft's owner can remove participants."
    );
    Response(StatusCodes.Status404NotFound, "Guest draft, guest drafter or participant not found.");
  }
}
