using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions.ClearDraftPositionAssignment;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Clears the participant assignment for a draft position.";
    Description = "This endpoint allows you to clear the participant assignment for a specific draft position.";
    Response(StatusCodes.Status204NoContent, "The participant assignment was successfully cleared.");
    Response(StatusCodes.Status400BadRequest, "The request was invalid. Please check the provided data and try again.");
    Response(StatusCodes.Status401Unauthorized, "You are not authorized to perform this action.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to perform this action.");
    Response(StatusCodes.Status404NotFound, "The specified draft position was not found.");
  }
}
