using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.AddHostToDraftPart;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Adds a host to a draft part.";
    Description = "Adds a host with a specified role to the draft part identified by the provided draft part ID.";
    Response(StatusCodes.Status204NoContent, "Host successfully added to the draft part.");
    Response(StatusCodes.Status400BadRequest, "Invalid request data.");
    Response(StatusCodes.Status401Unauthorized, "User is not authenticated.");
    Response(StatusCodes.Status403Forbidden, "User does not have permission to add a host to the draft part.");
    Response(StatusCodes.Status404NotFound, "Draft part or host not found.");
  }
}
