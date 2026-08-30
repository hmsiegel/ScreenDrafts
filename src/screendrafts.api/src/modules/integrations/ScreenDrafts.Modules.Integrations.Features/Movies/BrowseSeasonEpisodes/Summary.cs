using FastEndpoints;

namespace ScreenDrafts.Modules.Integrations.Features.Movies.BrowseSeasonEpisodes;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Browse every episode in a TV season.";
    Description =
      "Lists all episodes for the given TMDb series and season number, for use "
      + "as an episode-drafting candidate picklist. TMDb has no working keyword "
      + "search over episode titles, so this replaces free-text search for "
      + "episode-based drafts — the caller browses a season and picks from what's "
      + "actually in it.";
    Response(StatusCodes.Status200OK, "Episodes found.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized.");
    Response(StatusCodes.Status404NotFound, "No such series or season on TMDb.");
  }
}
