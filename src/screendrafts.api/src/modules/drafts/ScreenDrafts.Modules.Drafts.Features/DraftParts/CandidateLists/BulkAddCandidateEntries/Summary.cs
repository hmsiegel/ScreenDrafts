using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.BulkAddCandidateEntries;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Bulk add candidate list entries to a candidate list draft part.";
    Description = "This endpoint allows you to bulk add entries to a candidate list within a draft part.";
    Response<BulkAddCandidateEntriesResponse>(StatusCodes.Status200OK, "The candidate list entries were successfully added.");
    Response(StatusCodes.Status400BadRequest, "The request was invalid. This can occur if the CSV file is malformed or contains invalid data.");
    Response(StatusCodes.Status401Unauthorized, "The user is not authenticated.");
    Response(StatusCodes.Status403Forbidden, "The user does not have permission to update the candidate list.");
    Response(StatusCodes.Status404NotFound, "The specified draft part was not found.");
  }
}
