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

  public async Task<Person?> FindByTmdbAsync(
    int tmdbId,
    CancellationToken cancellationToken = default
  )
  {
    return await _dbContext.People.FirstOrDefaultAsync(p => p.TmdbId == tmdbId, cancellationToken);
  }
}
