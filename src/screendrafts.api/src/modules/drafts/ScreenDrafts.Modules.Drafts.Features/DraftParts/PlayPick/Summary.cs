using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.PlayPick;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Play a pick in a draft part.";
    Description = "Plays a pick in the specified draft part and updates the draft part's state accordingly.";
    Response<Guid>(StatusCodes.Status200OK, "The pick was played successfully.");
    Response(StatusCodes.Status400BadRequest, "The request was invalid.");
    Response(StatusCodes.Status401Unauthorized, "The user is not authenticated.");
    Response(StatusCodes.Status403Forbidden, "The user does not have permission to play a pick.");
    Response(StatusCodes.Status404NotFound, "The specified draft part was not found.");
  }
}
