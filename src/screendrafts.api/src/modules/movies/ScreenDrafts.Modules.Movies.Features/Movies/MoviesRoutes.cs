namespace ScreenDrafts.Modules.Movies.Features.Movies;

internal static class MoviesRoutes
{
  internal const string Base = "/movies";
  internal const string GetMovie = Base + "/{imdbId}";
  internal const string GetMovieSummary = Base + "/{imdbId}/summary";

  internal const string MovieSearch = "/draft-parts/{draftPartId}/movies/search";
}
