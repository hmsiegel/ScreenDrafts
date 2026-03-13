using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions.ListDraftPositions;

internal sealed class ListDraftPositionsSummary : Summary<Endpoint>
{
  public ListDraftPositionsSummary()
  {
    Summary = "List draft positions for a draft part.";
    Description = "Lists all draft positions for a specific draft part, including details such as assigned participants and their order in the draft.";
    Response<ListDraftPositionsResponse>(StatusCodes.Status200OK, "Successfully retrieved the list of draft positions.");
    Response(StatusCodes.Status400BadRequest, "The request was invalid. Check the error message for details.");
    Response(StatusCodes.Status401Unauthorized, "Authentication is required to access this resource.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource.");
    Response(StatusCodes.Status404NotFound, "The specified draft part was not found.");
  }
}
