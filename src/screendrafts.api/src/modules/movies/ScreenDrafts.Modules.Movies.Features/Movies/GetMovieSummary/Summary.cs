using FastEndpoints;

namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMovieSummary;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Get a movie summary by IMDb ID.";
    Description = "Gets a movie summary by IMDb ID.";
    Response<GetMovieSummaryResponse>(StatusCodes.Status200OK, "The movie summary was retrieved successfully.");
    Response(StatusCodes.Status404NotFound, "The movie summary was not found.");
  }
}
