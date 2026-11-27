using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Participants.SetPhantomParticipant;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Mark a drafter on a draft part as a phantom drafter, or clear the mark.";
    Description =
      "A phantom drafter cancelled at the last minute and only gave a pick (or nothing). "
      + "A phantom gets no starting veto, does not count as an appearance, and any veto rolling in "
      + "carries through to their next part. Clearing the mark is only allowed before the part starts.";
    Response(StatusCodes.Status204NoContent, "Phantom flag updated.");
    Response(StatusCodes.Status400BadRequest, "Invalid request data.");
    Response(StatusCodes.Status403Forbidden, "User does not have permission to update the draft part.");
    Response(StatusCodes.Status404NotFound, "Draft part or participant not found.");
  }
}
