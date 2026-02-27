using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.AssignParticipantToDraftPosition;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Assign a participant to a draft position.";
    Description = "Assigns a drafter, drafter team, or community participant to a draft position.";
    Response(StatusCodes.Status204NoContent, "Participant successfully assigned to the draft position.");
    Response(StatusCodes.Status400BadRequest, "Invalid request data, such as an invalid participant kind or public ID.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized access to the endpoint.");
    Response(StatusCodes.Status403Forbidden, "Forbidden access to the endpoint.");
    Response(StatusCodes.Status404NotFound, "The specified draft part or draft position was not found.");
    Response(StatusCodes.Status409Conflict, "A conflict occurred while assigning the participant to the draft position.");
  }
}
