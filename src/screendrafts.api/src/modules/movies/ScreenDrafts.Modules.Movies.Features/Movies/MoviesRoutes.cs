namespace ScreenDrafts.Modules.Movies.Features.Movies;

internal static class MoviesRoutes
{
  internal const string Base = "/media";
  internal const string GetMedia = Base + "/{publicId}";
  internal const string GetMediaSummary = Base + "/{publicId}/summary";
  internal const string MediaSearch = "/media/search";
  internal const string MediaByTmdbIds = Base + "/by-tmdb-ids";
  internal const string MediaByImdbIds = Base + "/by-imdb-ids";
  internal const string MediaByExternalIds = Base + "/by-external-ids";
  internal const string GamesMediaSearch = Base + "/search-games";
  internal const string MediaByIgdbIds = Base + "/by-igdb-ids";
  internal const string YouTubeMediaSearch = Base + "/search-youtube";
  internal const string PersonFilmographyMedia = Base + "/person-filmography";
}
