using ScreenDrafts.Common.Abstractions.Errors;

namespace ScreenDrafts.Modules.Integrations.Domain.Movies;

public static class MovieErrors
{
  public static readonly SDError SearchQueryRequired = SDError.Problem(
        "Movies.SearchQueryRequired",
        "A search query is required to search for movies.");

  public static readonly SDError UnsupportedMediaType = SDError.Problem(
        "Movies.UnsupportedMediaType",
        "The specified media type is not supported.");

  public static readonly SDError EpisodeFieldsAreRequired = SDError.Problem(
        "Movies.EpisodeFieldsAreRequired",
        "Episode fields are required.");

  public static readonly SDError IgdbIdIsRequired = SDError.Problem(
        "Movies.IgdbIdIsRequired",
        "The IGDB ID is required.");

  public static readonly SDError ImdbIdIsRequiredForMusicVideo = SDError.Problem(
    "Movies.ImdbIdIsRequiredForMusicVideos",
    "An IMDb ID is required for music videos.");

  public static SDError NotFound(string imdbId) => SDError.NotFound(
    "Movies.NotFound",
    $"The movie with the IMDB ID {imdbId} was not found.");

  public static SDError NotFound(int tmdbId) => SDError.NotFound(
    "Movies.NotFound",
    $"The movie with the TMDB ID {tmdbId} was not found.");
}
