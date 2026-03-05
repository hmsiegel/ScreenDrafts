using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.UndoPick;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Undo a pick for a draft part.";
    Description = "This endpoint allows you to undo a pick for a specific draft part. This is a specific use case and should only be used if the pick was truly made in error.";
    Response(StatusCodes.Status204NoContent, "The pick was successfully undone.");
    Response(StatusCodes.Status400BadRequest, "The request was invalid. This can occur if the pick already has a veto or commissioner override attached to it");
    Response(StatusCodes.Status401Unauthorized, "You are not authenticated. Please log in to undo this pick.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to undo this pick. Only the commissioner can undo it.");
    Response(StatusCodes.Status409Conflict, "The pick cannot be undone because it has already been vetoed or overridden by the commissioner.");
    Response(StatusCodes.Status404NotFound, "The specified draft part or pick was not found.");
  }
}
