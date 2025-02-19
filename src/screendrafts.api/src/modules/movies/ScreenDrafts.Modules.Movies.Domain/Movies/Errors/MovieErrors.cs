namespace ScreenDrafts.Modules.Movies.Domain.Movies.Errors;

public static class MovieErrors
{
  public static SDError MovieAlreadyExists(string imdbId) =>
    SDError.Conflict(
      "Movie.MovieAlreadyExists",
      $"The movie with the IMDB ID '{imdbId}' already exists.");
}
