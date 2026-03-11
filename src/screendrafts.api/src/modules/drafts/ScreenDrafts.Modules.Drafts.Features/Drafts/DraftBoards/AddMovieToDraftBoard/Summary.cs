using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards.AddMovieToDraftBoard;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Add a movie to a draft board.";
    Description = "Adds a movie to a user's draft board.";
    Response(StatusCodes.Status204NoContent, "Movie added to draft board successfully.");
    Response(StatusCodes.Status400BadRequest, "Invalid request data.");
    Response(StatusCodes.Status401Unauthorized, "User is not authenticated.");
    Response(StatusCodes.Status403Forbidden, "User does not have permission to modify this draft board.");
    Response(StatusCodes.Status404NotFound, "Draft board or movie not found.");
  }
}
