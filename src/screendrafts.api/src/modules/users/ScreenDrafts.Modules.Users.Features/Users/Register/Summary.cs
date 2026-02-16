using FastEndpoints;

namespace ScreenDrafts.Modules.Users.Features.Users.Register;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Register a new user";
    Description = "Register a new user";
    Response<Guid>(StatusCodes.Status200OK, "User registered successfully");
    Response(StatusCodes.Status400BadRequest, "Invalid request data");
    Response<Microsoft.AspNetCore.Mvc.ProblemDetails>(StatusCodes.Status500InternalServerError, "Internal server error");
  }
}
