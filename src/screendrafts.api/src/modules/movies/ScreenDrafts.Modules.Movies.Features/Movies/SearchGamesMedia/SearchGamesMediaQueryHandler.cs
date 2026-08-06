namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchGamesMedia;

internal sealed class SearchGamesMediaQueryHandler(
  IIntegrationsApi integrationsApi,
  IMediaRepository mediaRepository
) : IQueryHandler<SearchGamesMediaQuery, SearchGamesMediaResponse>
{
  private readonly IIntegrationsApi _integrationsApi = integrationsApi;
  private readonly IMediaRepository _mediaRepository = mediaRepository;

  public async Task<Result<SearchGamesMediaResponse>> Handle(
    SearchGamesMediaQuery request,
    CancellationToken cancellationToken
  )
  {
    if (string.IsNullOrWhiteSpace(request.Query))
    {
      return Result.Failure<SearchGamesMediaResponse>(MediaErrors.SearchQueryRequired);
    }

    var searchResult = await _integrationsApi.SearchGamesAsync(
      request.Query,
      request.Page,
      cancellationToken
    );

    if (searchResult.IsFailure)
    {
      return Result.Failure<SearchGamesMediaResponse>(searchResult.Errors);
    }

    var igdbIds = searchResult.Value.Results.Select(r => r.IgdbId).ToList();

    var existingIgdbIds =
      igdbIds.Count > 0
        ? await _mediaRepository.GetExistingMediaIgdbsAsync(igdbIds, cancellationToken)
        : [];

    var publicIdsByIgdbId =
      igdbIds.Count > 0
        ? await _mediaRepository.GetPublicIdsByIgdbIdsAsync(igdbIds, cancellationToken)
        : [];

    var items = searchResult
      .Value.Results.Select(r =>
      {
        var isInDatabase = existingIgdbIds.Contains(r.IgdbId);

        return new GameMediaSearchResultResponse
        {
          IgdbId = r.IgdbId,
          Title = r.Title,
          Year = r.Year,
          PosterUrl = r.Poster,
          IsInMediaDatabase = isInDatabase,
          MediaPublicId = isInDatabase ? publicIdsByIgdbId.GetValueOrDefault(r.IgdbId) : null,
        };
      })
      .ToList()
      .AsReadOnly();

    return Result.Success(new SearchGamesMediaResponse { Results = items });
  }
}
