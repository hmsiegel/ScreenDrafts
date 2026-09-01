using FastEndpoints;

namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.InviteParticipant;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Invite a participant to a guest draft";
    Description =
      "Adds a registered user as a participant in the specified guest draft, before it has started.";
    Response(StatusCodes.Status204NoContent, "The participant was successfully invited.");
    Response(StatusCodes.Status400BadRequest, "Invalid InviteParticipantRequest.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to invite participants.");
    Response(StatusCodes.Status404NotFound, "Guest draft not found.");
  }
}
