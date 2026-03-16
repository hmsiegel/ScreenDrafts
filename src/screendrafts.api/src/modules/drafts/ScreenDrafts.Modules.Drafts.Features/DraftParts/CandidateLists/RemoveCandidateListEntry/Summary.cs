using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.RemoveCandidateListEntry;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Removes an entry from a candidate list.";
    Description = "Removes an entry from a candidate list.";
    Response(StatusCodes.Status204NoContent, "The entry was successfully removed from the candidate list.");
    Response(StatusCodes.Status400BadRequest, "The request was invalid.");
    Response(StatusCodes.Status401Unauthorized, "The user is not authenticated.");
    Response(StatusCodes.Status403Forbidden, "The user does not have permission to remove entries from the candidate list.");
    Response(StatusCodes.Status404NotFound, "The candidate list or entry was not found.");
  }
}
