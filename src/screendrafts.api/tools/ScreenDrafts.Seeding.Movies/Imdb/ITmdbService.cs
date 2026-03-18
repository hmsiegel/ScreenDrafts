namespace ScreenDrafts.Seeding.Movies.Imdb;

public interface ITmdbService
{
  /// <summary>
  /// Search TMDB by Title. 
  /// </summary>
  /// <param name="query"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<IReadOnlyList<TmdbMovieSearchResult>> SearchMoviesAsync(
    string query,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Find a movie by its TMDB Id.
  /// </summary>
  /// <param name="imdbId"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<TmdbMovieSearchResult?> FindMovieByImdbIdAsync(
    string imdbId,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Fetch a full movie detail including cast and crew.
  /// </summary>
  /// <param name="tmdbId"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<TmdbMovieDetails?> GetMovieDetailsAsync(
    int tmdbId,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Resolve TMDB ID => IMDB ID.
  /// </summary>
  /// <param name="tmdbId"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<string?> GetImdbIdAsync(
    int tmdbId,
    CancellationToken cancellationToken = default);

  Task<string?> GetPersonImdbIdAsync(int tmdbPersonId,
    CancellationToken cancellationToken = default);

  Uri? BuildPosterUrl(string? posterPath, string size = "w500");
}
