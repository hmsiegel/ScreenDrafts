namespace ScreenDrafts.Modules.Movies.Domain.Movies.Errors;

public static class MovieErrors
{
  public static SDError MovieAlreadyExists(string imdbId) =>
    SDError.Conflict(
      "Movie.MovieAlreadyExists",
      $"The movie with the IMDB ID '{imdbId}' already exists.");

  public static SDError MovieNotFound(string imdbId) =>
    SDError.NotFound(
      "Movie.MovieNotFound",
      $"The movie with the IMDB ID '{imdbId}' was not found.");

  public static readonly SDError RequiredFieldsMissing =
    SDError.Failure(
      "Movie.RequiredFieldsMissing",
      "The title and IMDB ID are required fields.");
}
