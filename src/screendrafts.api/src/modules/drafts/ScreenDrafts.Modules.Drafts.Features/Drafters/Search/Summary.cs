using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Drafters.Search;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Search drafters.";
    Description =
      "Searches for drafters based on the provided criteria and returns a paginated list of matching drafters.";
    Response<PagedResult<SearchDraftersResponse>>(
      StatusCodes.Status200OK,
      "A paginated list of matching drafters."
    );
    Response(StatusCodes.Status400BadRequest, "The request was invalid.");
    Response(StatusCodes.Status401Unauthorized, "The user is not authenticated.");
    Response(
      StatusCodes.Status403Forbidden,
      "The user does not have permission to access this resource."
    );
  }
}
