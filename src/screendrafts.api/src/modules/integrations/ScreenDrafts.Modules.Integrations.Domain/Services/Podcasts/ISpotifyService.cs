namespace ScreenDrafts.Modules.Integrations.Domain.Services.Podcasts;

public interface ISpotifyService
{
  /// <summary>
  /// Episodes of the configured show. Includes every episode released on or
  /// after <paramref name="releasedOnOrAfter"/>; may include older ones from
  /// the last page fetched. Throws HttpRequestException when Spotify refuses
  /// or fails, so callers can tell "no episodes" from "lookup failed".
  /// </summary>
  Task<IReadOnlyList<PodcastEpisode>> ListEpisodesAsync(
    DateOnly releasedOnOrAfter,
    CancellationToken cancellationToken = default
  );
}
