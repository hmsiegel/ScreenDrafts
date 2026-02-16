using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Categories.Delete;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Delete a category by its public ID.";
    Description = "Deletes a category identified by its public ID. Requires appropriate permissions.";
    Response(StatusCodes.Status204NoContent, "Category deleted successfully.");
    Response(StatusCodes.Status404NotFound, "Category not found.");
    Response(StatusCodes.Status403Forbidden, "Forbidden.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized.");
  }
}
