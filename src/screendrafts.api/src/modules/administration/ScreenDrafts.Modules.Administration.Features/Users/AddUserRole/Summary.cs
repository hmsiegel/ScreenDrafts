namespace ScreenDrafts.Modules.Administration.Features.Users.AddUserRole;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Add user role";
    Description = "Add user role";
    Response(StatusCodes.Status204NoContent, "Role added successfully");
    Response(StatusCodes.Status400BadRequest, "Bad request");
    Response(StatusCodes.Status403Forbidden, "Forbidden");
    Response(StatusCodes.Status404NotFound, "Not found");
    Response(StatusCodes.Status500InternalServerError, "Internal server error");
  }
}
