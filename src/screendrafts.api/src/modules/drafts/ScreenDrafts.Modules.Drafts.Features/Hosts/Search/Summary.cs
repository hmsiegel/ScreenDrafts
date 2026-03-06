using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Hosts.Search;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Search hosts with optional filters and pagination.";
    Description = "Search for hosts with optional filters such as name and whether they have been primary hosts. Supports pagination and sorting.";
    Response<PagedResult<SearchHostResponse>>(StatusCodes.Status200OK, "A paginated list of hosts matching the search criteria.");
    Response(StatusCodes.Status400BadRequest, "Invalid search parameters.");
    Response(StatusCodes.Status401Unauthorized, "Authentication is required.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource.");
  }
}
