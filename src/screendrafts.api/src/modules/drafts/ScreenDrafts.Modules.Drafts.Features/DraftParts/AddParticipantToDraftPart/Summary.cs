using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.AddParticipantToDraftPart;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Add a participant to a draft part.";
    Description = "Adds a participant (drafter, drafter team, or community) to a specific draft part. The participant will be associated with the draft part and can participate in the drafting process.";
    Response(StatusCodes.Status204NoContent, "Participant added successfully.");
    Response(StatusCodes.Status400BadRequest, "Invalid request data.");
    Response(StatusCodes.Status403Forbidden, "User does not have permission to add a participant to the draft part.");
    Response(StatusCodes.Status404NotFound, "Draft part or participant not found.");
  }
}

