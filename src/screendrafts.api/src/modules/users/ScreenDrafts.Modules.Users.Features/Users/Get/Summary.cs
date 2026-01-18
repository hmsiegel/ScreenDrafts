namespace ScreenDrafts.Modules.Users.Features.Users.Get;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Description = "Get the current users' profile.";
    Summary = "Get user profile.";
    Response<Response>(StatusCodes.Status200OK, "The user's profile was successfully retrieved.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized");
    Response(StatusCodes.Status403Forbidden, "Forbidden");
  }
}
