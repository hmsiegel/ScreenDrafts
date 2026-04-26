using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.RevealPick;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Reveal a pick to all participants.";
    Description = "Reveals a previously submitted pick, making it visible to all draft participants. Only the primary host may call this endpoint.";
    Response(StatusCodes.Status204NoContent, "The pick was revealed successfully.");
    Response(StatusCodes.Status400BadRequest, "The request was invalid.");
    Response(StatusCodes.Status401Unauthorized, "The user is not authenticated.");
    Response(StatusCodes.Status403Forbidden, "The user does not have permission to reveal picks.");
    Response(StatusCodes.Status404NotFound, "The specified draft part or pick was not found.");
  }
}
