namespace ScreenDrafts.Modules.Integrations.Features.YouTube.SearchYouTube;

internal sealed record YouTubeSearchResultItem
{
  public string VideoId { get; init; } = string.Empty;
  public string Title { get; init; } = string.Empty;
  public string? ChannelTitle { get; init; }
  public string? ThumbnailUrl { get; init; }
}
