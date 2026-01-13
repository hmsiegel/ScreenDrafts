namespace ScreenDrafts.Modules.Drafts.Features.Categories.List;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Lists all categories.";
    Description = "Returns a reference list of categories, sorted by name.";
    Response<CategoryCollectionResponse>(StatusCodes.Status200OK, "The list of categories.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized");
    Response(StatusCodes.Status403Forbidden, "Forbidden");
  }
}
