namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchMovies;

internal sealed class SearchMoviesQueryHandler(
  IIntegrationsApi integrationsApi,
  IMovieRepository movieRepository)
  : IQueryHandler<SearchMoviesQuery, SearchMoviesResponse>
{
  private readonly IIntegrationsApi _integrationsApi = integrationsApi;
  private readonly IMovieRepository _movieRepository = movieRepository;

  public async Task<Result<SearchMoviesResponse>> Handle(SearchMoviesQuery request, CancellationToken cancellationToken)
  {
    if (string.IsNullOrWhiteSpace(request.Query))
    {
      return Result.Failure<SearchMoviesResponse>(MovieErrors.SearchQueryRequired);
    }

    var searchResults = await _integrationsApi.SearchMoviesAsync(
      request.Query,
      cancellationToken);

    if (searchResults.IsFailure)
    {
      return Result.Failure<SearchMoviesResponse>(searchResults.Errors);
    }

    var filtered = searchResults.Value.Results.AsEnumerable();

    if (request.Year.HasValue)
    {
      filtered = filtered.Where(r =>
        r.Year is not null &&
        int.TryParse(r.Year, out var year) &&
        year == request.Year.Value);
    }

    var filteredList = filtered.ToList();
    var totalCount = filteredList.Count;

    // Pagination
    var paged = filteredList
      .Skip((request.Page - 1) * request.PageSize)
      .Take(request.PageSize)
      .ToList();

    // Bulk DB check against paged slice only
    var tmdbIds = paged.Select(r => r.TmdbId).ToList();
    var existingTmdbIds = await _movieRepository.GetExistingMovieTmdbsAsync(
      tmdbIds,
      cancellationToken);

    var items = paged.Select(r => new MovieSearchResultResponse
    {
      TmdbId = r.TmdbId,
      Title = r.Title,
      Year = r.Year,
      PosterUrl = r.Poster,
      Overview = r.Overview,
      IsInMoviesDatabase = existingTmdbIds.Contains(r.TmdbId)
    })
      .ToList()
      .AsReadOnly();

    return Result.Success(new SearchMoviesResponse
    {
      Results = new PagedResult<MovieSearchResultResponse>
      {
        Items = items,
        Page = request.Page,
        PageSize = request.PageSize,
        TotalCount = totalCount
      }
    });

  }
}
