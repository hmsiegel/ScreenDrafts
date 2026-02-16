using FastEndpoints;

namespace ScreenDrafts.Modules.Users.Features.Users.Update;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Updates the current user's profile information.";
    Description = "Updates the current user's profile information.";
    Response(StatusCodes.Status204NoContent, "The user profile was updated successfully.");
    Response(StatusCodes.Status400BadRequest, "The request was invalid.");
    Response(StatusCodes.Status401Unauthorized, "The user is not authenticated.");
    Response(StatusCodes.Status403Forbidden, "The user does not have permission to update the profile.");
  }
}
