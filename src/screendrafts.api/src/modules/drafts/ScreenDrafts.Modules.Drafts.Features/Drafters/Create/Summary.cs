namespace ScreenDrafts.Modules.Drafts.Features.Drafters.Create;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Create a new drafter";
    Description = "Create a new drafter. This endpoint creates a new drafter with the specified person ID.";
    Response<string>(StatusCodes.Status201Created, "The ID of the newly created drafter.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status401Unauthorized, "Authentication is required to access this resource.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource.");
    Response(StatusCodes.Status409Conflict, "A drafter with the specified details already exists.");
  }
}
