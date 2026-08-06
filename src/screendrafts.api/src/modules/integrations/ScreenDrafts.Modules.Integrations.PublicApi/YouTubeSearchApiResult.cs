namespace ScreenDrafts.Modules.Integrations.PublicApi;

public sealed record YouTubeSearchApiResult
{
  public string VideoId { get; init; } = default!;
  public string Title { get; init; } = default!;
  public string? ChannelTitle { get; init; }
  public Uri? ThumbnailUrl { get; init; }
}
