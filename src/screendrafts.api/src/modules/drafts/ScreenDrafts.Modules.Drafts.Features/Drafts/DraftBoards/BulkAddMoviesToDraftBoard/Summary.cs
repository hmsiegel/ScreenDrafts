using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards.BulkAddMoviesToDraftBoard;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Bulk add movies to a draft board.";
    Description = "This endpoint allows you to bulk add movies to a draft board using a CSV file.";
    Response<BulkAddMoviesResponse>(StatusCodes.Status200OK, "Movies added successfully.");
    Response(StatusCodes.Status400BadRequest, "Invalid request. The CSV file is missing or improperly formatted.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized. You must be authenticated to access this endpoint.");
    Response(StatusCodes.Status403Forbidden, "Forbidden. You do not have permission to add movies to this draft board.");
    Response(StatusCodes.Status404NotFound, "Draft board not found.");
  }
}
