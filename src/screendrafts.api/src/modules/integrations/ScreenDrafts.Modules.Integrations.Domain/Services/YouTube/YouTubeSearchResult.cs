namespace ScreenDrafts.Modules.Integrations.Domain.Services.YouTube;

/// <summary>
/// Lightweight search result — mirrors TmdbSearchResult's role. No duration
/// here; search.list doesn't return it, and paying for a second call per
/// result during search would be wasteful. Duration only gets fetched via
/// GetVideoDetailsAsync once a specific result is selected.
/// </summary>
public sealed record YouTubeSearchResult
{
  public string VideoId { get; init; } = string.Empty;
  public string Title { get; init; } = string.Empty;
  public string? ChannelTitle { get; init; }
  public Uri? ThumbnailUrl { get; init; }
  public string? PublishedAt { get; init; }
}
