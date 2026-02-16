using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Categories.Edit;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Edits an existing category.";
    Description = "Edits an existing category.";
    ExampleRequest = new EditCategoryRequest
    {
      Name = "New Category Name",
      Description = "New Category Description",
    };
    Response(StatusCodes.Status204NoContent, "Category edited successfully.");
    Response(StatusCodes.Status400BadRequest, "Invalid EditCategoryRequest.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized.");
    Response(StatusCodes.Status403Forbidden, "Forbidden.");
    Response(StatusCodes.Status404NotFound, "Category not found.");
  }
}

