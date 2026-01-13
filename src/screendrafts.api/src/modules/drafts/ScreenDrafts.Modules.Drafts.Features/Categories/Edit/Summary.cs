namespace ScreenDrafts.Modules.Drafts.Features.Categories.Edit;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Edits an existing category.";
    Description = "Edits an existing category.";
    ExampleRequest = new Request
    {
      Name = "New Category Name",
      Description = "New Category Description",
    };
    Response(StatusCodes.Status204NoContent, "Category edited successfully.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized.");
    Response(StatusCodes.Status403Forbidden, "Forbidden.");
    Response(StatusCodes.Status404NotFound, "Category not found.");
  }
}
