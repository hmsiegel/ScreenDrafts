namespace ScreenDrafts.Modules.Integrations.Domain.Services.YouTube;

public sealed record YouTubeVideoDetails
{
  public string VideoId { get; init; } = string.Empty;
  public string Title { get; init; } = string.Empty;
  public string? Description { get; init; }
  public string? ChannelTitle { get; init; }
  public Uri? ThumbnailUrl { get; init; }
  public string? PublishedAt { get; init; }
  public int DurationSeconds { get; init; }

  /// <summary>
  /// Heuristic, not an official YouTube flag — the public Data API doesn't
  /// expose "is this a Short" directly. Duration ≤ 180s is a reasonable
  /// proxy (YouTube's own Shorts cutoff is 180s as of the multi-minute
  /// Shorts rollout), but a short-but-not-a-Short video will misclassify.
  /// Good enough for staging a Speed Draft subject; not authoritative.
  /// </summary>
  public bool LikelyShort => DurationSeconds > 0 && DurationSeconds <= 180;
}
