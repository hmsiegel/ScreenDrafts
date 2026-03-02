using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ListDrafts;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "List drafts with pagination and filtering options.";
    Description = "List drafts with pagination and filtering options. Supports various query parameters for filtering and sorting the results.";
    Response<PagedResult<ListDraftsResponse>>(200, "A paginated list of drafts matching the specified criteria.");
    Response(400, "Bad request. The request parameters are invalid.");
  }
}
