namespace ScreenDrafts.Modules.Movies.Domain.Medias.Repositories;

public interface IMediaRepository : IRepository
{
  void Add(Media media);
  Task<bool> ExistsByIgdbIdAsync(int igdbId, CancellationToken cancellationToken = default);
  Task<bool> ExistsByTmdbIdAsync(int tmdbId, MediaType mediaType, CancellationToken cancellationToken = default);
  Task<Media?> FindByImdbIdAsync(string imdbId, CancellationToken cancellationToken = default);
  Task<Media?> FindByTmdbIdAsync(int tmdbId, MediaType mediaType, CancellationToken cancellationToken = default);
  Task<Media?> FindByIgdbIdAsync(int igdbId, CancellationToken cancellationToken = default);
  void AddMediaActor(Media media, Person actor);
  void AddMediaDirector(Media media, Person director);
  void AddMediaWriter(Media media, Person writer);
  void AddMediaProducer(Media media, Person producer);
  void AddMediaGenre(Media media, Genre genre);
  void AddMediaProductionCompany(Media media, ProductionCompany productionCompany);
  Task<HashSet<string>> GetExistingMediaImdbsAsync(IEnumerable<string> imdbIds, CancellationToken cancellationToken = default);
  Task<HashSet<(int TmdbId, int MediaTypeValue)>> GetExistingMediaTmdbsAsync(IEnumerable<int> tmdbIds, CancellationToken cancellationToken = default);
  Task<HashSet<int>> GetExistingMediaIgdbsAsync(IEnumerable<int> igdbIds, CancellationToken cancellationToken = default);
}


