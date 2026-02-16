namespace ScreenDrafts.Modules.Administration.Features.Users.RemoveUserRole;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Remove user role";
    Description = "Remove user role";
    Response(StatusCodes.Status204NoContent, "User role removed successfully");
    Response(StatusCodes.Status404NotFound, "User not found");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized");
  }
}
