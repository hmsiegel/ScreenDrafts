namespace ScreenDrafts.Modules.Integrations.Domain.Services.Tmdb;

public interface ITmdbService
{
  /// <summary>
  /// Find any media type by IMDb ID.
  /// Returns movie, TV show or TV episode result depending on what TMDb finds.
  /// Returns null result if nothing matches.
  /// </summary>
  /// <param name="imdbId"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<TmdbFindResult?> FindByImdbIdAsync(
    string imdbId,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Search TMDB by Title (movies only).
  /// </summary>
  /// <param name="query"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<IReadOnlyList<TmdbSearchResult>> SearchMoviesAsync(
    string query,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Find a movie by its TMDB Id.
  /// </summary>
  /// <param name="imdbId"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<TmdbSearchResult?> FindMovieByImdbIdAsync(
    string imdbId,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Fetch a full movie detail including cast, crew and trailer.
  /// </summary>
  /// <param name="tmdbId"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<TmdbMediaDetails?> GetMovieDetailsAsync(
    int tmdbId,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Fetch  full TV show details including cast, crew and trailer.
  /// </summary>
  /// <param name="tmdbId"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<TmdbMediaDetails?> GetTvShowDetailsAsync(
    int tmdbId,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Fetch TV episode details including episode-level credits.
  /// Requires the series TMDB ID, season number and episode number to fetch the episode-level credits.
  /// </summary>
  /// <param name="seriesTmdbId"></param>
  /// <param name="seasonNumber"></param>
  /// <param name="episodeNumber"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<TmdbMediaDetails?> GetTvEpisodeDetailsAsync(
    int seriesTmdbId,
    int seasonNumber,
    int episodeNumber,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Resolve TMDB ID => IMDB ID.
  /// </summary>
  /// <param name="tmdbId"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<string?> GetMovieImdbIdAsync(
    int tmdbId,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Resovle TMDb ID => IMDB ID for TV shows.
  /// </summary>
  /// <param name="tmdbId"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<string?> GetTvShowImdbIdAsync(
    int tmdbId,
    CancellationToken cancellationToken = default);

  Task<string?> GetPersonImdbIdAsync(
    int tmdbPersonId,
    CancellationToken cancellationToken = default);

  Uri? BuildPosterUrl(string? posterPath, string size = "w500");
}
