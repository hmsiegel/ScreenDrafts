namespace ScreenDrafts.Modules.Users.Features.Users.Register;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Register a new user";
    Description = "Register a new user";
    Response<string>(StatusCodes.Status200OK, "User registered successfully");
    Response(StatusCodes.Status400BadRequest, "Invalid request data");
    Response<ProblemDetails>(StatusCodes.Status500InternalServerError, "Internal server error");
  }
}
