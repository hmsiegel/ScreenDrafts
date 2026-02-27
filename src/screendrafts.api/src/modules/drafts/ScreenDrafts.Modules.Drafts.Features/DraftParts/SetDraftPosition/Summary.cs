using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SetDraftPosition;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Sets the draft positions for a draft part.";
    Description = "Sets the draft positions for a draft part. These are the positions that participants can be assigned to during the draft and include the pick positions for each participant.";
    Response(StatusCodes.Status204NoContent, "Draft positions successfully set.");
    Response(StatusCodes.Status400BadRequest, "Invalid request data.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized access.");
    Response(StatusCodes.Status403Forbidden, "Forbidden access.");
    Response(StatusCodes.Status404NotFound, "Draft part not found.");
  }
}
