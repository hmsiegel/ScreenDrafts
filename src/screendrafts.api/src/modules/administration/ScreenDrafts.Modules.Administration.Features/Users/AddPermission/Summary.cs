using FastEndpoints;

namespace ScreenDrafts.Modules.Administration.Features.Users.AddPermission;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Adds a new permission.";
    Description = "Adds a new permission to the databese.";
    Response(StatusCodes.Status204NoContent, "Permission added successfully.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "User does not have permission to add permissions.");
  }
}
