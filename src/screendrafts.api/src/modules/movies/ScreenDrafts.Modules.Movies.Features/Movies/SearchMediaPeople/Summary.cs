using FastEndpoints;

namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchMediaPeople;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Search the people credited on titles in the Vault.";
    Description =
      "Returns people whose name contains the query, with how many Vault titles they are "
      + "credited on in the requested role (any, actor, director, writer, producer). "
      + "Feeds the person picker on the media list.";
    Response<SearchMediaPeopleResponse>(
      StatusCodes.Status200OK,
      "People retrieved successfully."
    );
  }
}
