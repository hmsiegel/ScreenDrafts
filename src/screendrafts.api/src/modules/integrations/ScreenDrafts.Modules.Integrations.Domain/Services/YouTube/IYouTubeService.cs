namespace ScreenDrafts.Modules.Integrations.Domain.Services.YouTube;

public interface IYouTubeService
{
  /// <summary>
  /// Search by free-text query, video results only (type=video). Used as
  /// the source for Speed Draft music-video/short subjects, since neither
  /// TMDb nor OMDb carries this content.
  /// </summary>
  Task<YouTubeSearchPagedResult> SearchAsync(
    string query,
    int page = 1,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Full detail for a single video, including duration — needed to
  /// classify MusicVideo vs Short, since search results alone don't carry
  /// duration (a separate videos.list call is required either way).
  /// </summary>
  Task<YouTubeVideoDetails?> GetVideoDetailsAsync(
    string videoId,
    CancellationToken cancellationToken = default
  );
}
