namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchYouTubeMedia;

internal sealed class SearchYouTubeMediaQueryHandler(
  IIntegrationsApi integrationsApi,
  IMediaRepository mediaRepository
) : IQueryHandler<SearchYouTubeMediaQuery, SearchYouTubeMediaResponse>
{
  private readonly IIntegrationsApi _integrationsApi = integrationsApi;
  private readonly IMediaRepository _mediaRepository = mediaRepository;

  public async Task<Result<SearchYouTubeMediaResponse>> Handle(
    SearchYouTubeMediaQuery request,
    CancellationToken cancellationToken
  )
  {
    if (string.IsNullOrWhiteSpace(request.Query))
    {
      return Result.Failure<SearchYouTubeMediaResponse>(MediaErrors.SearchQueryRequired);
    }

    var searchResult = await _integrationsApi.SearchYouTubeAsync(
      request.Query,
      request.Page,
      cancellationToken
    );

    if (searchResult.IsFailure)
    {
      return Result.Failure<SearchYouTubeMediaResponse>(searchResult.Errors);
    }

    var videoIds = searchResult.Value.Results.Select(r => r.VideoId).ToList();

    var existingExternalIds =
      videoIds.Count > 0
        ? await _mediaRepository.GetExistingMediaExternalIdsAsync(videoIds, cancellationToken)
        : [];

    var publicIdsByExternalId =
      videoIds.Count > 0
        ? await _mediaRepository.GetPublicIdsByExternalIdsAsync(videoIds, cancellationToken)
        : [];

    var items = searchResult
      .Value.Results.Select(r =>
      {
        var isInDatabase = existingExternalIds.Contains(r.VideoId);

        return new YouTubeMediaSearchResultResponse
        {
          ExternalId = r.VideoId,
          Title = r.Title,
          ChannelTitle = r.ChannelTitle,
          ThumbnailUrl = r.ThumbnailUrl?.ToString(),
          IsInMediaDatabase = isInDatabase,
          MediaPublicId = isInDatabase ? publicIdsByExternalId.GetValueOrDefault(r.VideoId) : null,
        };
      })
      .ToList()
      .AsReadOnly();

    return Result.Success(new SearchYouTubeMediaResponse { Results = items });
  }
}
