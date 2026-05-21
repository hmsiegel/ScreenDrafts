using FastEndpoints;

namespace ScreenDrafts.Modules.Movies.Features.Movies.ListMedia;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "List media with pagination, search, and filters.";
    Description =
      "Returns a paginated list of all media in the ScreenDrafts database. "
      + "Filter by media type (0=Movie, 1=TvShow, 2=TvEpisode, 3=VideoGame) or year. "
      + "Sort by title or year.";
    Response<ListMediaResponse>(StatusCodes.Status200OK, "Media list retrieved successfully.");
  }
}
