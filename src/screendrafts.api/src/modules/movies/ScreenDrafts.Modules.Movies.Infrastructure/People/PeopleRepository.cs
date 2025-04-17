namespace ScreenDrafts.Modules.Movies.Infrastructure.People;

internal sealed class PeopleRepository(MoviesDbContext dbContext)
  : MoviesRepositoryBase<Person>(dbContext), IPersonRepository
{
  private readonly MoviesDbContext _dbContext = dbContext;

  public void Add(Person person)
  {
    if (_dbContext.Entry(person).State == EntityState.Detached)
    {
      _dbContext.People.Attach(person);
    }

    _dbContext.People.Add(person);
  }

  public async Task<Person?> FindByImdbIdAsync(string imdbId, CancellationToken cancellationToken = default)
  {
    return await _dbContext.People
      .FirstOrDefaultAsync(p => p.ImdbId == imdbId, cancellationToken);
  }
}
