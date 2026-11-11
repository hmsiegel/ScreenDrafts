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

  Task<Result<ListPodcastEpisodesApiResponse>> ListPodcastEpisodesAsync(
    PodcastPlatform platform,
    DateOnly releasedOnOrAfter,
    CancellationToken cancellationToken = default
  );
}
