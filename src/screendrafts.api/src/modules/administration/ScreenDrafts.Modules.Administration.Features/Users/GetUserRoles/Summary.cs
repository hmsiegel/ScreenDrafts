using FastEndpoints;

namespace ScreenDrafts.Modules.Administration.Features.Users.GetUserRoles;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Get a user's roles";
    Description = "Get a user's roles by their public ID.";
    Response<GetUserRolesResponse>(StatusCodes.Status200OK, "The user's roles were successfully retrieved.");
    Response(StatusCodes.Status400BadRequest, "The request was invalid.");
    Response(StatusCodes.Status401Unauthorized, "The user is not authenticated.");
    Response(StatusCodes.Status403Forbidden, "The user does not have permission to access this resource.");
    Response(StatusCodes.Status404NotFound, "The user was not found.");
  }
}
