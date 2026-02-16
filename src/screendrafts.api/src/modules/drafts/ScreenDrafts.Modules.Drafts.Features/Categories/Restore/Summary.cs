using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Categories.Restore;

internal sealed class Summary : Summary<Endpoint>

{
  public Summary()
  {
    Summary = "Restores a deleted category.";
    Description = "Restores a deleted category by its unique identifier.";
    Responses = new()
    {
      { StatusCodes.Status204NoContent, "The category was successfully restored." },
      { StatusCodes.Status400BadRequest, "The RestoreCategoryRequest was invalid." },
      { StatusCodes.Status401Unauthorized, "The user is not authenticated." },
      { StatusCodes.Status403Forbidden, "The user does not have permission to restore categories." },
      { StatusCodes.Status404NotFound, "The category with the specified ID was not found." }
    };
  }
}

