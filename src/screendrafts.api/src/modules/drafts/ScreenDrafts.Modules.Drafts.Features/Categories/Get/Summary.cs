using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Categories.Get;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Get a category by its public ID.";
    Description = "Retrieves the details of a specific category using its public identifier.";
    Response<CategoryResponse>(StatusCodes.Status200OK, "The category was found and returned successfully.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized access.");
    Response(StatusCodes.Status403Forbidden, "Forbidden access.");
  }
}
