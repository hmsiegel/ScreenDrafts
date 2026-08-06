namespace ScreenDrafts.Modules.Movies.Domain.Medias.Repositories;

public interface IMediaRepository : IRepository
{
  void Add(Media media);
  Task<bool> ExistsByIgdbIdAsync(int igdbId, CancellationToken cancellationToken = default);
  Task<bool> ExistsByExternalIdAsync(
    string externalId,
    CancellationToken cancellationToken = default
  );
  Task<bool> ExistsByTmdbIdAsync(
    int tmdbId,
    MediaType mediaType,
    CancellationToken cancellationToken = default
  );
  Task<Media?> FindByImdbIdAsync(string imdbId, CancellationToken cancellationToken = default);
  Task<Media?> FindByTmdbIdAsync(
    int tmdbId,
    MediaType mediaType,
    CancellationToken cancellationToken = default
  );
  Task<Media?> FindByIgdbIdAsync(int igdbId, CancellationToken cancellationToken = default);
  void AddMediaActor(Media media, Person actor);
  void AddMediaDirector(Media media, Person director);
  void AddMediaWriter(Media media, Person writer);
  void AddMediaProducer(Media media, Person producer);
  void AddMediaGenre(Media media, Genre genre);
  void AddMediaProductionCompany(Media media, ProductionCompany productionCompany);
  Task<HashSet<string>> GetExistingMediaImdbsAsync(
    IEnumerable<string> imdbIds,
    CancellationToken cancellationToken = default
  );
  Task<HashSet<(int TmdbId, int MediaTypeValue)>> GetExistingMediaTmdbsAsync(
    IEnumerable<int> tmdbIds,
    CancellationToken cancellationToken = default
  );
  Task<HashSet<int>> GetExistingMediaIgdbsAsync(
    IEnumerable<int> igdbIds,
    CancellationToken cancellationToken = default
  );
  Task<HashSet<string>> GetExistingMediaExternalIdsAsync(
    IEnumerable<string> externalIds,
    CancellationToken cancellationToken = default
  );
  Task<Dictionary<(int TmdbId, int MediaTypeValue), string>> GetPublicIdsByTmdbIdsAsync(
    IEnumerable<(int TmdbId, int MediaTypeValue)> tmdbIds,
    CancellationToken cancellationToken = default
  );
  Task<Dictionary<string, string>> GetPublicIdsByImdbIdsAsync(
    IEnumerable<string> imdbIds,
    CancellationToken cancellationToken = default
  );
  Task<Dictionary<int, string>> GetPublicIdsByIgdbIdsAsync(
    IEnumerable<int> igdbIds,
    CancellationToken cancellationToken = default
  );
  Task<Dictionary<string, string>> GetPublicIdsByExternalIdsAsync(
    IEnumerable<string> externalIds,
    CancellationToken cancellationToken = default
  );

  Task<Media?> FindByTvEpisodeAsync(
    int tvSeriesTmdbId,
    int seasonNumber,
    int episodeNumber,
    CancellationToken cancellationToken = default
  );

  Task<Media?> FindByTmdbIdForUpdateAsync(
    int tmdbId,
    MediaType mediaType,
    CancellationToken cancellationToken = default
  );

  Task<Media?> FindByTvEpisodeForUpdateAsync(
    int tvSeriesTmdbId,
    int seasonNumber,
    int episodeNumber,
    CancellationToken cancellationToken = default
  );
}
