using ScreenDrafts.Common.Abstractions.Results;

namespace ScreenDrafts.Modules.Integrations.PublicApi;

public interface IIntegrationsApi
{
  Task<Result<SearchMediaApiResponse>> SearchMoviesAsync(
    string query,
    int page = 1,
    CancellationToken cancellationToken = default
  );

  Task<Result<SearchGamesApiResponse>> SearchGamesAsync(
    string query,
    int page = 1,
    CancellationToken cancellationToken = default
  );

  Task<Result<SearchYouTubeApiResponse>> SearchYouTubeAsync(
    string query,
    int page = 1,
    CancellationToken cancellationToken = default
  );

  Task<Result<PersonFilmographyApiResponse>> GetPersonFilmographyAsync(
    string imdbId,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// A person's name and photo from TMDb. Looks up by TMDb ID when it is above zero, otherwise
  /// by IMDb ID (people imported before TMDb IDs were stored have only the latter).
  /// </summary>
  Task<Result<PersonProfileApiResponse>> GetPersonProfileAsync(
    int tmdbId,
    string? imdbId,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Fetches a title's directors, cast, writers, producers and production companies from TMDb.
  /// Feeds the people resync, which re-attaches them to a title already in the Vault.
  /// </summary>
  Task<Result<OnlineMediaCreditsApiResponse>> GetOnlineMediaCreditsAsync(
    OnlineMediaCreditsApiRequest request,
    CancellationToken cancellationToken = default
  );

  Task<Result<ListPodcastEpisodesApiResponse>> ListPodcastEpisodesAsync(
    PodcastPlatform platform,
    DateOnly releasedOnOrAfter,
    CancellationToken cancellationToken = default
  );
}
