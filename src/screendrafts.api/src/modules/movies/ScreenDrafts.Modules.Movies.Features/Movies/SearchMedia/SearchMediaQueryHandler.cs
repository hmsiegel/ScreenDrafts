namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchMedia;

internal sealed class SearchMediaQueryHandler(
  IIntegrationsApi integrationsApi,
  IMediaRepository movieRepository
) : IQueryHandler<SearchMediaQuery, SearchMediaResponse>
{
  private readonly IIntegrationsApi _integrationsApi = integrationsApi;
  private readonly IMediaRepository _movieRepository = movieRepository;

  public async Task<Result<SearchMediaResponse>> Handle(
    SearchMediaQuery request,
    CancellationToken cancellationToken
  )
  {
    if (string.IsNullOrWhiteSpace(request.Query))
    {
      return Result.Failure<SearchMediaResponse>(MediaErrors.SearchQueryRequired);
    }

    var searchResults = await _integrationsApi.SearchMoviesAsync(
      request.Query,
      request.Page,
      cancellationToken
    );

    if (searchResults.IsFailure)
    {
      return Result.Failure<SearchMediaResponse>(searchResults.Errors);
    }

    var filtered = searchResults.Value.Results.AsEnumerable();

    if (request.Year.HasValue)
    {
      filtered = filtered.Where(r =>
        r.Year is not null && int.TryParse(r.Year, out var year) && year == request.Year.Value
      );
    }

    var filteredList = filtered.ToList();

    var tmdbIds = filteredList.Where(r => r.TmdbId.HasValue).Select(r => r.TmdbId!.Value).ToList();

    // Pairs for the public-id lookup — same source rows as tmdbIds above,
    // but keeping mediaType alongside since the same numeric TMDb ID can
    // belong to both a movie and a show.
    var tmdbIdMediaTypePairs = filteredList
      .Where(r => r.TmdbId.HasValue)
      .Select(r => (TmdbId: r.TmdbId!.Value, MediaTypeValue: r.MediaType.Value))
      .Distinct()
      .ToList();

    var igdbIds = filteredList.Where(r => r.IgdbId.HasValue).Select(r => r.IgdbId!.Value).ToList();

    // OMDb-fallback results have no TmdbId, only ImdbId — same merge treatment
    // as tmdb/igdb, just keyed differently since imdb_id needs no MediaType
    // disambiguation.
    var imdbIds = filteredList
      .Where(r => !r.TmdbId.HasValue && !string.IsNullOrWhiteSpace(r.ImdbId))
      .Select(r => r.ImdbId!)
      .ToList();

    var existingTmdbIds =
      tmdbIds.Count > 0
        ? await _movieRepository.GetExistingMediaTmdbsAsync(tmdbIds, cancellationToken)
        : [];

    var existingIgdbIds =
      igdbIds.Count > 0
        ? await _movieRepository.GetExistingMediaIgdbsAsync(igdbIds, cancellationToken)
        : [];

    var existingImdbIds =
      imdbIds.Count > 0
        ? await _movieRepository.GetExistingMediaImdbsAsync(imdbIds, cancellationToken)
        : [];

    var publicIdsByTmdbId =
      tmdbIdMediaTypePairs.Count > 0
        ? await _movieRepository.GetPublicIdsByTmdbIdsAsync(tmdbIdMediaTypePairs, cancellationToken)
        : [];

    var publicIdsByImdbId =
      imdbIds.Count > 0
        ? await _movieRepository.GetPublicIdsByImdbIdsAsync(imdbIds, cancellationToken)
        : [];

    var items = filteredList
      .Select(r =>
      {
        var isInDatabase =
          (r.TmdbId.HasValue && existingTmdbIds.Contains((r.TmdbId.Value, r.MediaType)))
          || (r.IgdbId.HasValue && existingIgdbIds.Contains(r.IgdbId.Value))
          || (!r.TmdbId.HasValue && r.ImdbId is not null && existingImdbIds.Contains(r.ImdbId));

        var publicId = isInDatabase switch
        {
          false => null,
          true when r.TmdbId.HasValue => publicIdsByTmdbId.GetValueOrDefault(
            (r.TmdbId.Value, r.MediaType.Value)
          ),
          true when r.ImdbId is not null => publicIdsByImdbId.GetValueOrDefault(r.ImdbId),
          _ => null,
        };

        return new MediaSearchResultResponse
        {
          ImdbId = r.ImdbId,
          TmdbId = r.TmdbId,
          IgdbId = r.IgdbId,
          Title = r.Title,
          Year = r.Year,
          PosterUrl = r.Poster,
          Overview = r.Overview,
          MediaType = r.MediaType,
          IsInMediaDatabase = isInDatabase,
          MediaPublicId = publicId,
        };
      })
      .ToList()
      .AsReadOnly();

    return Result.Success(
      new SearchMediaResponse
      {
        Results = new PagedResult<MediaSearchResultResponse>
        {
          Items = items,
          Page = request.Page,
          PageSize = request.PageSize,
          TotalCount = searchResults.Value.TotalCount,
        },
      }
    );
  }
}
