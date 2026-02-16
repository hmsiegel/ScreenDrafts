using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.People.Search;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Search the database for people who match the specific SearchPeopleQuery.";
    Description = "This endpoint allows you to search for people in the database based on a search SearchPeopleQuery. It returns a list of people that match the search criteria, limited by the specified number.";
    Response<PeopleSearchResponse>(StatusCodes.Status200OK, "A list of people matching the search SearchPeopleQuery.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized access.");
    Response(StatusCodes.Status403Forbidden, "Forbidden access.");
  }
}

