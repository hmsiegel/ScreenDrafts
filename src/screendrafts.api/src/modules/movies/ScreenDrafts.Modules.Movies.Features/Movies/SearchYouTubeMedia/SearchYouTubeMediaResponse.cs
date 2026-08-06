namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchYouTubeMedia;

internal sealed record SearchYouTubeMediaResponse
{
  public IReadOnlyList<YouTubeMediaSearchResultResponse> Results { get; init; } = [];
}
