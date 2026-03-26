namespace ScreenDrafts.Modules.Movies.Domain.Medias.Errors;

public static class MediaErrors
{
  public static SDError MediaAlreadyExists(string imdbId) =>
    SDError.Conflict(
      "Media.MediaAlreadyExists",
      $"The media with the IMDB ID '{imdbId}' already exists.");

  public static SDError MediaAlreadyExists(int tmdbId) =>
    SDError.Conflict(
      "Media.MediaAlreadyExists",
      $"The media with the TMDB ID '{tmdbId}' already exists.");

  public static SDError MediaNotFound(string publicId) =>
    SDError.NotFound(
      "Media.MediaNotFound",
      $"The media with the Public ID '{publicId}' was not found.");

  public static SDError MediaNotFound(int tmdbId) =>
    SDError.NotFound(
      "Media.MediaNotFound",
      $"The media with the TMDB ID '{tmdbId}' was not found.");

  public static readonly SDError RequiredFieldsMissing =
    SDError.Failure(
      "Media.RequiredFieldsMissing",
      "The title and IMDB ID are required fields.");

  public static readonly SDError SearchQueryRequired =
      SDError.Failure(
        "Media.SearchQueryRequired",
        "The search query is required.");

  public static readonly SDError IgdbIdRequiredForVideoGames =
    SDError.Failure(
      "Media.IgdbIdRequiredForVideoGames",
      "The IGDB ID is required for video games.");

  public static readonly SDError TmdbIdRequired =
    SDError.Failure(
      "Media.TmdbIdRequired",
      "The TMDB ID is required for movies, TV shows, and TV episodes.");

  public static readonly SDError EpisodeFieldsRequired =
    SDError.Failure(
      "Media.EpisodeFieldsRequired",
      "The TV episode fields are required.");

  public static readonly SDError PublicIdRequired =
    SDError.Failure(
      "Media.PublicIdRequired",
      "Public Id is required");
}
