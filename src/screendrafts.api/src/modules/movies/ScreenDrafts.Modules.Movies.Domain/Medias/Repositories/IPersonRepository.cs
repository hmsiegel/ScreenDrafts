namespace ScreenDrafts.Modules.Movies.Domain.Medias.Repositories;

public interface IPersonRepository : IRepository
{
  Task<Person?> FindByImdbIdAsync(string imdbId, CancellationToken cancellationToken = default);
  Task<Person?> FindByTmdbAsync(int tmdbId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Legacy people (tmdb_id = 0, IMDb id set) already credited on a title in any role.
  /// Lets a resync reuse them instead of creating a second person for the same human.
  /// </summary>
  Task<IReadOnlyList<Person>> GetLegacyPeopleOnMediaAsync(
    Guid mediaId,
    CancellationToken cancellationToken = default
  );

  void Add(Person person);
}
