using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.ApplyCommissionerOverride;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Applies the commissioner override to the specified draft pick.";
    Description = "This endpoint allows the commissioners to remove a pick from the board that they feel strays too far from the topic. This is at no penalty to the drafter.";
    Response(StatusCodes.Status204NoContent, "The commissioner override was successfully applied.");
    Response(StatusCodes.Status400BadRequest, "The request was invalid. This can occur if the draft part is not in a state that allows commissioner overrides, or if the play order is invalid.");
    Response(StatusCodes.Status401Unauthorized, "The user is not authenticated.");
    Response(StatusCodes.Status403Forbidden, "The user does not have permission to apply commissioner overrides.");
    Response(StatusCodes.Status404NotFound, "The specified draft part was not found.");
  }
}
