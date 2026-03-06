using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Hosts.Get;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Get a host by its public ID.";
    Description = "Retrieves a host's details using its public ID. Requires authentication and appropriate permissions.";
    Response(StatusCodes.Status200OK, "Successfully retrieved the host details.");
    Response(StatusCodes.Status400BadRequest, "The request was invalid. Check the provided public ID and try again.");
    Response(StatusCodes.Status401Unauthorized, "Authentication is required to access this endpoint.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to access this endpoint.");
    Response(StatusCodes.Status404NotFound, "No host found with the specified public ID.");
  }
}
