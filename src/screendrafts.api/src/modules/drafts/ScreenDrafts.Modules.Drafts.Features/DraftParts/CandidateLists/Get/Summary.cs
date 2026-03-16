using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.Get;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Gets the candidate list for a specific draft part.";
    Description = "Gets the candidate list for a specific draft part.";
    Response<GetCandidateListResponse>(StatusCodes.Status200OK, "The candidate list was retrieved successfully.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized access.");
    Response(StatusCodes.Status403Forbidden, "Forbidden access.");
    Response(StatusCodes.Status404NotFound, "The specified draft part was not found.");
  }
}
