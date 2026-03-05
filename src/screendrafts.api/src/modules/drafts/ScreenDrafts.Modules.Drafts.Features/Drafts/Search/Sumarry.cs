using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.Search;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Search drafts with pagination and optional filters.";
    Description = "Search for drafts based on name, campaign, category, and draft type. Supports pagination.";
    Response<PagedResult<SearchDraftsResponse>>(200, "Successful search with results.");
    Response(400, "Invalid request parameters.");
  }
}
