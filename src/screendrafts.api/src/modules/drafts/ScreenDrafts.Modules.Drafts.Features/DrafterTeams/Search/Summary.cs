using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.Search;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Search drafter teams.";
    Description = "Search drafter teams.";
    Response<PagedResult<SearchDrafterTeamsResponse>>(StatusCodes.Status200OK, "Success");
    Response(StatusCodes.Status400BadRequest, "Bad Request");
  }
}
