namespace ScreenDrafts.Modules.Drafts.Features.People.Create;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Create a new person";
    Description = "Creates a new person with the specified details.";
    Response<string>(StatusCodes.Status201Created, "The public ID of the newly created person.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status401Unauthorized, "Authentication is required.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource.");
  }
}
