namespace ScreenDrafts.Modules.Administration.Features.Users.AddPermissionToRole;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Add permission to role";
    Description = "Add permission to role";
    Response(StatusCodes.Status204NoContent, "No content");
    Response(StatusCodes.Status403Forbidden, "Forbidden");
    Response(StatusCodes.Status400BadRequest, "Bad request");
    Response(StatusCodes.Status404NotFound, "Not found");
    Response(StatusCodes.Status500InternalServerError, "Internal server error");
  }
}
