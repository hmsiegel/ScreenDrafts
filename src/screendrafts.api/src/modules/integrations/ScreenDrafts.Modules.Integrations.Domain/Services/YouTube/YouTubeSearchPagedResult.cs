namespace ScreenDrafts.Modules.Integrations.Domain.Services.YouTube;

public sealed record YouTubeSearchPagedResult
{
  public IReadOnlyList<YouTubeSearchResult> Results { get; init; } = [];
  public string? NextPageToken { get; init; }
}
