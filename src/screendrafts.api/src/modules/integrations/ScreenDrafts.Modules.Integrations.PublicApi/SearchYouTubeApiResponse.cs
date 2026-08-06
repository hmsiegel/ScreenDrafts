namespace ScreenDrafts.Modules.Integrations.PublicApi;

public sealed record SearchYouTubeApiResponse
{
  public IReadOnlyList<YouTubeSearchApiResult> Results { get; init; } = [];
}
