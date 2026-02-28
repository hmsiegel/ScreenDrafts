using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.RemoveParticipantFromDraftPart;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Remove a participant from a draft part.";
    Description = "Removes a participant (drafter, drafter team, or community) from a specific draft part. The participant will no longer be associated with the draft part and cannot participate in the drafting process.";
    Response(StatusCodes.Status204NoContent, "Participant removed successfully.");
    Response(StatusCodes.Status400BadRequest, "Invalid request data.");
    Response(StatusCodes.Status403Forbidden, "User does not have permission to remove a participant from the draft part.");
    Response(StatusCodes.Status404NotFound, "Draft part or participant not found.");
  }
}

