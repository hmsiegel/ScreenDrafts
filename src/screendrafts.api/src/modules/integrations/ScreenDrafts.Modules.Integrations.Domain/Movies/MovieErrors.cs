using ScreenDrafts.Common.Abstractions.Errors;

namespace ScreenDrafts.Modules.Integrations.Domain.Movies;

public static class MovieErrors
{
  public static readonly SDError SearchQueryRequired = SDError.Problem(
        "Movies.SearchQueryRequired",
        "A search query is required to search for movies.");

  public static SDError NotFound(string imdbId) => SDError.NotFound(
    "Movies.NotFound",
    $"The movie with the IMDB ID {imdbId} was not found.");
}
