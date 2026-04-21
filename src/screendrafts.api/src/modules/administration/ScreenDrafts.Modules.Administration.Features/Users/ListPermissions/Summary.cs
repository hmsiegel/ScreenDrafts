using FastEndpoints;

namespace ScreenDrafts.Modules.Administration.Features.Users.ListPermissions;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "List all permissions.";
    Description = "Lists all permissions that can be assigned to users.";
    Response<ListPermissionsResponse>(StatusCodes.Status200OK, "Permissions retrieved successfully.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized access.");
    Response(StatusCodes.Status403Forbidden, "Forbidden access.");
  }
}
