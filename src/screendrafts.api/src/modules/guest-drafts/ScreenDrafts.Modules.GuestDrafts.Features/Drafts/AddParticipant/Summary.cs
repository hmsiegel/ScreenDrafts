using FastEndpoints;
using ScreenDrafts.Modules.GuestDrafts.Features.Drafts.AddParticipant;

namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.AddParticipant;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Add a participant to a guest draft";
    Description =
      "Owner-only. Adds a registered guest drafter as a participant, before the draft has started. The owner must add themselves too, like anyone else -- creating a draft no longer implies participating in it.";
    Response(StatusCodes.Status204NoContent, "Participant added.");
    Response(
      StatusCodes.Status400BadRequest,
      "Invalid AddParticipantRequest, or the draft has already started."
    );
    Response(StatusCodes.Status403Forbidden, "Only the guest draft's owner can add participants.");
    Response(StatusCodes.Status404NotFound, "Guest draft or guest drafter not found.");
  }
}
