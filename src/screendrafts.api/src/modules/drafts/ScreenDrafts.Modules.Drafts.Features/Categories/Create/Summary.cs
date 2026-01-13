namespace ScreenDrafts.Modules.Drafts.Features.Categories.Create;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Create a new category";
    Description = "Creates a new category with the specified parameters.";
    Response<CreatedResponse>(StatusCodes.Status201Created, "The PublicId of the created category.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to create a category.");
  }
}
