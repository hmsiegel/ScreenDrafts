namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchYouTubeMedia;

internal sealed record YouTubeMediaSearchResultResponse
{
  public string ExternalId { get; init; } = default!;
  public string Title { get; init; } = default!;
  public string? ChannelTitle { get; init; }
  public string? ThumbnailUrl { get; init; }
  public bool IsInMediaDatabase { get; init; }
  public string? MediaPublicId { get; init; }
}
