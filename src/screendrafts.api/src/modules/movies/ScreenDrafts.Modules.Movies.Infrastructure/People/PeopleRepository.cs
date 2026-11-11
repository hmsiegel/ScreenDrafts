namespace ScreenDrafts.Modules.Movies.Infrastructure.People;

internal sealed class PeopleRepository(MoviesDbContext dbContext)
  : MoviesRepositoryBase<Person>(dbContext),
    IPersonRepository
{
  private readonly MoviesDbContext _dbContext = dbContext;

  public void Add(Person person)
  {
    _dbContext.People.Add(person);
  }

  public async Task<Person?> FindByImdbIdAsync(
    string imdbId,
    CancellationToken cancellationToken = default
  )
  {
    return await _dbContext.People.FirstOrDefaultAsync(p => p.ImdbId == imdbId, cancellationToken);
  }

  public async Task<IReadOnlyList<Person>> GetLegacyPeopleOnMediaAsync(
    Guid mediaId,
    CancellationToken cancellationToken = default
  )
  {
    var id = MediaId.Create(mediaId);

    // Four small primary-key lookups, then one people query. Keeps the SQL plain: a UNION over
    // the strongly typed id columns is not worth the translation risk.
    var actorIds = await _dbContext
      .MediaActors.Where(x => x.MediaId == id)
      .Select(x => x.ActorId)
      .ToListAsync(cancellationToken);
    var directorIds = await _dbContext
      .MediaDirectors.Where(x => x.MediaId == id)
      .Select(x => x.DirectorId)
      .ToListAsync(cancellationToken);
    var writerIds = await _dbContext
      .MediaWriters.Where(x => x.MediaId == id)
      .Select(x => x.WriterId)
      .ToListAsync(cancellationToken);
    var producerIds = await _dbContext
      .MediaProducers.Where(x => x.MediaId == id)
      .Select(x => x.ProducerId)
      .ToListAsync(cancellationToken);

    var personIds = actorIds
      .Concat(directorIds)
      .Concat(writerIds)
      .Concat(producerIds)
      .Distinct()
      .ToList();

    if (personIds.Count == 0)
    {
      return [];
    }

    return await _dbContext
      .People.Where(p =>
        p.TmdbId == 0 && p.ImdbId != null && p.ImdbId != "" && personIds.Contains(p.Id)
      )
      .ToListAsync(cancellationToken);
  }

  public async Task<Person?> FindByTmdbAsync(
    int tmdbId,
    CancellationToken cancellationToken = default
  )
  {
    return await _dbContext.People.FirstOrDefaultAsync(p => p.TmdbId == tmdbId, cancellationToken);
  }
}
