using ScreenDrafts.Modules.Integrations.Domain.Services.YouTube;

namespace ScreenDrafts.Modules.Integrations.Features.YouTube.SearchYouTube;

internal sealed class SearchYouTubeCommandHandler(IYouTubeService youTubeService)
  : ICommandHandler<SearchYouTubeCommand, SearchYouTubeResponse>
{
  private readonly IYouTubeService _youTubeService = youTubeService;

  public async Task<Result<SearchYouTubeResponse>> Handle(
    SearchYouTubeCommand request,
    CancellationToken cancellationToken
  )
  {
    if (string.IsNullOrWhiteSpace(request.Query))
    {
      return Result.Failure<SearchYouTubeResponse>(MovieErrors.SearchQueryRequired);
    }

    var searchResult = await _youTubeService.SearchAsync(
      request.Query,
      request.Page,
      cancellationToken
    );

    var mapped = searchResult
      .Results.Select(r => new YouTubeSearchResultItem
      {
        VideoId = r.VideoId,
        Title = r.Title,
        ChannelTitle = r.ChannelTitle,
        ThumbnailUrl = r.ThumbnailUrl?.ToString(),
      })
      .ToList()
      .AsReadOnly();

    return Result.Success(new SearchYouTubeResponse { Results = mapped });
  }
}
