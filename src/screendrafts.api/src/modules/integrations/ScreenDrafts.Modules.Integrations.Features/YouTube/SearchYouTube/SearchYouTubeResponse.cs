namespace ScreenDrafts.Modules.Integrations.Features.YouTube.SearchYouTube;

internal sealed record SearchYouTubeResponse
{
  public IReadOnlyList<YouTubeSearchResultItem> Results { get; init; } = [];
}
