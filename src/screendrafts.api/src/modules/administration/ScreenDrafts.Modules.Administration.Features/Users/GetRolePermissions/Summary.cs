using FastEndpoints;

namespace ScreenDrafts.Modules.Administration.Features.Users.GetRolePermissions;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Get permissions for a specific role.";
    Description = "Get a list of permissions associated with a specific role.";
    Response<GetRolePermissionsResponse>(
      StatusCodes.Status200OK,
      "A list of permissions for the specified role."
    );
    Response(StatusCodes.Status400BadRequest, "Invalid request parameters.");
    Response(
      StatusCodes.Status401Unauthorized,
      "Authentication is required and has failed or has not yet been provided."
    );
    Response(
      StatusCodes.Status403Forbidden,
      "The user does not have the necessary permissions to access this resource."
    );
    Response(StatusCodes.Status404NotFound, "The specified role was not found.");
  }
}
