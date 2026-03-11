using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards.UpdateDraftBoardItem;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Update a movie in a draft board.";
    Description = "Updates a movie in a user's draft board.";
    Response(StatusCodes.Status204NoContent, "Movie updated in draft board successfully.");
    Response(StatusCodes.Status400BadRequest, "Invalid request data.");
    Response(StatusCodes.Status401Unauthorized, "User is not authenticated.");
    Response(StatusCodes.Status403Forbidden, "User does not have permission to modify this draft board.");
    Response(StatusCodes.Status404NotFound, "Draft board or movie not found.");
  }
}
