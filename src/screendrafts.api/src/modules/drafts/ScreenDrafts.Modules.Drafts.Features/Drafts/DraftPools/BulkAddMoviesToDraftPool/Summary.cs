using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.BulkAddMoviesToDraftPool;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Bulk add movies to a draft pool.";
    Description = "Bulk add movies to a draft pool.";
    Response<BulkAddMoviesResponse>(StatusCodes.Status200OK, "Movies added to the draft pool successfully.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized.");
    Response(StatusCodes.Status403Forbidden, "Forbidden.");
    Response(StatusCodes.Status404NotFound, "Draft pool not found.");
  }
}
