using FastEndpoints;

namespace ScreenDrafts.Modules.Administration.Features.Users.GetPermissionByCode;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Get a permission by its code.";
    Description = "Gets a permission by its code. Requires the PermissionsRead permission.";
    Response<PermissionResponse>(StatusCodes.Status200OK, "The permission was found.");
    Response(StatusCodes.Status400BadRequest, "The request was invalid.");
    Response(StatusCodes.Status401Unauthorized, "The user is not authenticated.");
    Response(StatusCodes.Status403Forbidden, "The user does not have the required permissions.");
    Response(StatusCodes.Status404NotFound, "The permission was not found.");
  }
}
