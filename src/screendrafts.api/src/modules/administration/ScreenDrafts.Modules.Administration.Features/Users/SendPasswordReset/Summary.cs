using FastEndpoints;

namespace ScreenDrafts.Modules.Administration.Features.Users.SendPasswordReset;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Sends a password reset email to the user.";
    Description =
      "Sends a password reset email to the user, allowing them to reset their password securely.";
    Response(StatusCodes.Status204NoContent, "Password reset email sent successfully.");
    Response(StatusCodes.Status400BadRequest, "Invalid request. The provided data is not valid.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized. The user is not authenticated.");
    Response(
      StatusCodes.Status403Forbidden,
      "Forbidden. The user does not have permission to perform this action."
    );
    Response(StatusCodes.Status404NotFound, "Not Found. The specified user does not exist.");
  }
}
