namespace ScreenDrafts.Modules.Integrations.Domain.Services.Tmdb;

public interface ITmdbService
{
  /// <summary>
  /// Find any media type by IMDb ID.
  /// Returns movie, TV show or TV episode result depending on what TMDb finds.
  /// Returns null result if nothing matches.
  /// </summary>
  /// <param name="imdbId"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<TmdbFindResult?> FindByImdbIdAsync(
    string imdbId,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Search TMDB by Title (movies only).
  /// </summary>
  /// <param name="query"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<TmdbSearchPagedResult> SearchMoviesAsync(
    string query,
    int page = 1,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Search TMDB by title, TV shows only — GET /search/tv. Distinct from
  /// SearchMoviesAsync's endpoint; TMDb keeps movie and TV search separate.
  /// Used to pick which series a draft (or an episode draft's restriction)
  /// is about — not the same thing as GetSeasonEpisodesAsync, which lists
  /// episodes once a series is already known.
  /// </summary>
  Task<TmdbSearchPagedResult> SearchTvShowsAsync(
    string query,
    int page = 1,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Find a movie by its TMDB Id.
  /// </summary>
  /// <param name="imdbId"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<TmdbSearchResult?> FindMovieByImdbIdAsync(
    string imdbId,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Fetch a full movie detail including cast, crew and trailer.
  /// </summary>
  /// <param name="tmdbId"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<TmdbMediaDetails?> GetMovieDetailsAsync(
    int tmdbId,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Fetch  full TV show details including cast, crew and trailer.
  /// </summary>
  /// <param name="tmdbId"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<TmdbMediaDetails?> GetTvShowDetailsAsync(
    int tmdbId,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Lightweight lookup of just a TV series' display name — GET /tv/{series_id},
  /// with no append_to_response. Deliberately cheaper than GetTvShowDetailsAsync
  /// (which pulls credits + videos): this is called once per episode fetch just to
  /// stamp a human-readable series name onto the episode's Media record, and doesn't
  /// need anything else off the series. Returns null if TMDb has no such series.
  /// </summary>
  Task<string?> GetTvShowNameAsync(int tmdbId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Browse every episode in a season — GET /tv/{series_id}/season/{season_number}.
  /// This is a listing, not a search: TMDb has no working keyword search over episode
  /// titles, so candidate-list seeding for episode drafts is done by picking a season
  /// and letting the admin choose from what's actually in it, rather than typing a
  /// query. Returns an empty list if TMDb has no such season.
  /// </summary>
  Task<IReadOnlyList<TmdbSeasonEpisode>> GetSeasonEpisodesAsync(
    int seriesTmdbId,
    int seasonNumber,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Fetch TV episode details including episode-level credits.
  /// Requires the series TMDB ID, season number and episode number to fetch the episode-level credits.
  /// </summary>
  /// <param name="seriesTmdbId"></param>
  /// <param name="seasonNumber"></param>
  /// <param name="episodeNumber"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<TmdbMediaDetails?> GetTvEpisodeDetailsAsync(
    int seriesTmdbId,
    int seasonNumber,
    int episodeNumber,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Resolve TMDB ID => IMDB ID.
  /// </summary>
  /// <param name="tmdbId"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<string?> GetMovieImdbIdAsync(int tmdbId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Resovle TMDb ID => IMDB ID for TV shows.
  /// </summary>
  /// <param name="tmdbId"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<string?> GetTvShowImdbIdAsync(int tmdbId, CancellationToken cancellationToken = default);

  Task<string?> GetPersonImdbIdAsync(
    int tmdbPersonId,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Reverse of GetPersonImdbIdAsync — resolves an IMDb person ID (nm...) to
  /// a TMDb person ID via GET /find/{imdb_id}?external_source=imdb_id. Null
  /// if TMDb has no matching person record for that IMDb ID.
  /// </summary>
  Task<int?> FindPersonByImdbIdAsync(string imdbId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Full filmography (movies + TV, every credit type — cast and crew) via
  /// GET /person/{id}/combined_credits. Not filtered by role; the caller
  /// decides what to do with CreditRole.
  /// </summary>
  Task<IReadOnlyList<TmdbPersonCredit>> GetPersonCombinedCreditsAsync(
    int tmdbPersonId,
    CancellationToken cancellationToken = default
  );

  Uri? BuildPosterUrl(string? posterPath, string size = "w500");
}
