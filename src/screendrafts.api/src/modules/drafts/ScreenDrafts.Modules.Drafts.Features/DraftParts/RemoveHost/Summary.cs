using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.RemoveHost;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Remove a host from a draft part";
    Description = "Remove a host from a specific draft part. This endpoint allows you to remove a host from a draft part by providing the draft part ID and host ID.";
    Response(StatusCodes.Status204NoContent, "The host was successfully removed from the draft part.");
    Response(StatusCodes.Status404NotFound, "Draft part or host not found.");
    Response(StatusCodes.Status400BadRequest, "Invalid RemoveHostDraftPartRequest.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource.");
  }
}

