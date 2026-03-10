namespace ScreenDrafts.Modules.Integrations.IntegrationTests.Doubles;

public sealed class FakeTmdbService : ITmdbService
{
  private IReadOnlyList<TmdbMovieSearchResult> _searchResults = [];
  private TmdbMovieSearchResult? _findResult;
  private TmdbMovieDetails? _details;

  public void SetSearchResults(IReadOnlyList<TmdbMovieSearchResult> results) =>
    _searchResults = results;

  public void SetFindResult(TmdbMovieSearchResult? result) =>
    _findResult = result;

  public void SetDetails(TmdbMovieDetails? details) =>
    _details = details;

  public void Reset()
  {
    _searchResults = [];
    _findResult = null;
    _details = null;
  }

  public Task<IReadOnlyList<TmdbMovieSearchResult>> SearchMoviesAsync(
    string query,
    CancellationToken cancellationToken = default)
    => Task.FromResult(_searchResults);

  public Task<TmdbMovieSearchResult?> FindMovieByImdbIdAsync(
    string imdbId,
    CancellationToken cancellationToken = default)
    => Task.FromResult(_findResult);

  public Task<TmdbMovieDetails?> GetMovieDetailsAsync(
    int tmdbId,
    CancellationToken cancellationToken = default)
    => Task.FromResult(_details);

  public Task<string?> GetImdbIdAsync(
    int tmdbId,
    CancellationToken cancellationToken = default)
    => Task.FromResult<string?>(null);

  public Uri? BuildPosterUrl(string? posterPath, string size = "w500") =>
    posterPath is null
      ? null
      : new Uri($"https://image.tmdb.org/t/p/{size}{posterPath}");

  public Uri? BuildTrailerUrl(string? trailerPath, string size = "w500") =>
    trailerPath is null
      ? null
      : new Uri($"https://www.youtube.com/watch?v={trailerPath}");
}
