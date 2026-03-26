namespace ScreenDrafts.Modules.Integrations.IntegrationTests.Doubles;

public sealed class FakeTmdbService : ITmdbService
{
  private IReadOnlyList<TmdbSearchResult> _searchResults = [];
  private TmdbSearchResult? _findResult;
  private TmdbMediaDetails? _details;

  public void SetSearchResults(IReadOnlyList<TmdbSearchResult> results) =>
    _searchResults = results;

  public void SetFindResult(TmdbSearchResult? result) =>
    _findResult = result;

  public void SetDetails(TmdbMediaDetails? details) =>
    _details = details;

  public void Reset()
  {
    _searchResults = [];
    _findResult = null;
    _details = null;
  }

  public Task<IReadOnlyList<TmdbSearchResult>> SearchMoviesAsync(
    string query,
    CancellationToken cancellationToken = default)
    => Task.FromResult(_searchResults);

  public Task<TmdbSearchResult?> FindMovieByImdbIdAsync(
    string imdbId,
    CancellationToken cancellationToken = default)
    => Task.FromResult(_findResult);

  public Task<TmdbMediaDetails?> GetMovieDetailsAsync(
    int tmdbId,
    CancellationToken cancellationToken = default)
    => Task.FromResult(_details);

  public Task<string?> GetMovieImdbIdAsync(
    int tmdbId,
    CancellationToken cancellationToken = default)
    => Task.FromResult<string?>(null);

  public Uri? BuildPosterUrl(string? posterPath, string size = "w500") =>
    posterPath is null
      ? null
      : new Uri($"https://image.tmdb.org/t/p/{size}{posterPath}");

  public Task<TmdbMediaDetails?> GetTvShowDetailsAsync(
    int tmdbId,
    CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }

  public Task<TmdbMediaDetails?> GetTvEpisodeDetailsAsync(int seriesTmdbId, int seasonNumber, int episodeNumber, CancellationToken cancellationToken = default)
  {
    return Task.FromResult<TmdbMediaDetails?>(null);
  }

  public Task<string?> GetTvShowImdbIdAsync(int tmdbId, CancellationToken cancellationToken = default)
  {
    return Task.FromResult<string?>(null);
  }

  public Task<string?> GetPersonImdbIdAsync(int tmdbPersonId, CancellationToken cancellationToken = default)
  {
    return Task.FromResult<string?>(null);
  }

  public Task<TmdbFindResult?> FindByImdbIdAsync(string imdbId, CancellationToken cancellationToken = default)
  {
    return Task.FromResult<TmdbFindResult?>(null);
  }
}
