using FastEndpoints;

namespace ScreenDrafts.Modules.Reporting.Features.Drafts.SearchSpotlightCandidates;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Search spotlight candidates.";
    Description = "Search for spotlight candidates based on the provided query.";
    Response<SearchSpotlightCandidatesResponse>(
      StatusCodes.Status200OK,
      "Successfully retrieved spotlight candidates."
    );
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized.");
    Response(StatusCodes.Status403Forbidden, "Forbidden.");
    Response(StatusCodes.Status404NotFound, "Not found.");
  }
}
