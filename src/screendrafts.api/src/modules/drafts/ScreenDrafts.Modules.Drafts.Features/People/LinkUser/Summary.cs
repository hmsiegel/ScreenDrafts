
using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.People.LinkUser;

internal sealed class Summary : Summary<Endpoint>
  {
  public Summary()
  {
    Summary = "Link a user to a person.";
    Description = "Link a user to a person in the system.";
    Response(StatusCodes.Status204NoContent, "The user was successfully linked to the person.");
    Response(StatusCodes.Status400BadRequest, "The LinkUserPersonRequest was invalid.");
    Response(StatusCodes.Status401Unauthorized, "The user is not authenticated.");
    Response(StatusCodes.Status403Forbidden, "The user does not have permission to link a user to a person.");
  }
}

