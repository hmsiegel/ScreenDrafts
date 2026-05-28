using FastEndpoints;

namespace ScreenDrafts.Modules.Administration.Features.Users.GetRoles;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Get all available roles.";
    Description =
      "Gets a list of all available roles in the system. This endpoint is useful for displaying role options when assigning roles to users.";
    Response<GetRolesResponse>(
      StatusCodes.Status200OK,
      "A list of roles was successfully retrieved."
    );
    Response(
      StatusCodes.Status401Unauthorized,
      "Unauthorized. Authentication is required to access this endpoint."
    );
    Response(
      StatusCodes.Status403Forbidden,
      "Forbidden. The authenticated user does not have permission to access this endpoint."
    );
  }
}
