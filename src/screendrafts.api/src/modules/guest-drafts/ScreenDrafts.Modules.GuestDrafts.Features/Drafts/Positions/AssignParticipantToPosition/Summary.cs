using FastEndpoints;
using ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Positions.AssignParticipantToPosition;

namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Positions.AssignParticipantToPosition;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Assign a participant to a board position";
    Description =
      "Owner-only. Assigns a participant to a position, applying any bonus veto/override/fungible-token award the position carries.";
    Response(StatusCodes.Status204NoContent, "Participant assigned.");
    Response(
      StatusCodes.Status400BadRequest,
      "Position already assigned, or participant not part of this draft."
    );
    Response(StatusCodes.Status403Forbidden, "Only the guest draft's owner can assign positions.");
    Response(StatusCodes.Status404NotFound, "Guest draft, position, or participant not found.");
  }
}
