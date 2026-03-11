using ScreenDrafts.Common.Abstractions.Errors;

namespace ScreenDrafts.Modules.Movies.Domain.Movies.Errors;

public static class MovieErrors
{
  public static SDError MovieAlreadyExists(string imdbId) =>
    SDError.Conflict(
      "Movie.MovieAlreadyExists",
      $"The movie with the IMDB ID '{imdbId}' already exists.");

  public static SDError MovieAlreadyExists(int tmdbId) =>
    SDError.Conflict(
      "Movie.MovieAlreadyExists",
      $"The movie with the TMDB ID '{tmdbId}' already exists.");

  public static SDError MovieNotFound(string imdbId) =>
    SDError.NotFound(
      "Movie.MovieNotFound",
      $"The movie with the IMDB ID '{imdbId}' was not found.");

  public static SDError MovieNotFound(int tmdbId) =>
    SDError.NotFound(
      "Movie.MovieNotFound",
      $"The movie with the TMDB ID '{tmdbId}' was not found.");

  public static readonly SDError RequiredFieldsMissing =
    SDError.Failure(
      "Movie.RequiredFieldsMissing",
      "The title and IMDB ID are required fields.");

  public static readonly SDError SearchQueryRequired =
      SDError.Failure(
        "Movie.SearchQueryRequired",
        "The search query is required.");
}
