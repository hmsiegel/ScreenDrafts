using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.AddCandidateListEntry;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Add a candidate list entry to a draft part.";
    Description = "Adds a new entry to the candidate list of a specified draft part.";
    Response<AddCanidateEntryResponse>(StatusCodes.Status200OK, "The candidate list entry was successfully added.");
    Response(StatusCodes.Status400BadRequest, "The request was invalid. This can occur if the draft part ID is invalid or if the candidate list entry data is malformed.");
    Response(StatusCodes.Status401Unauthorized, "The user is not authorized to add a candidate list entry to this draft part.");
    Response(StatusCodes.Status403Forbidden, "The user does not have permission to add a candidate list entry to this draft part.");
    Response(StatusCodes.Status404NotFound, "The specified draft part was not found.");
  }
}
