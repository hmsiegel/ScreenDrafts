namespace ScreenDrafts.Modules.Integrations.Domain.Services.Podcasts;

public interface IApplePodcastsService
{
  /// <summary>
  /// Episodes of the configured show released on or after
  /// <paramref name="releasedOnOrAfter"/>. The iTunes lookup returns the
  /// newest episodes only, so older ones may be unreachable. Throws
  /// HttpRequestException when Apple refuses or fails.
  /// </summary>
  Task<IReadOnlyList<PodcastEpisode>> ListEpisodesAsync(
    DateOnly releasedOnOrAfter,
    CancellationToken cancellationToken = default
  );
}
