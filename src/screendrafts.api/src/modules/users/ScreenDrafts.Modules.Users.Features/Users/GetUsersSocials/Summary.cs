
namespace ScreenDrafts.Modules.Users.Features.Users.GetUsersSocials;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Get users' social media links by their person IDs.";
    Description = "Retrieves the social media links for multiple users based on their associated person IDs.";
    Response<Response>(StatusCodes.Status200OK, "Returns the users' social media links.");
    Response(StatusCodes.Status400BadRequest, "The request was invalid.");
    Response(StatusCodes.Status401Unauthorized, "The user is not authenticated.");
    Response(StatusCodes.Status403Forbidden, "The user does not have permission to access this resource.");
  }
}
