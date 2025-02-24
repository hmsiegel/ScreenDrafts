namespace ScreenDrafts.Modules.Movies.Infrastructure.People;

internal sealed class PeopleRepository(MoviesDbContext dbContext) : IPersonRepository
{
  private readonly MoviesDbContext _dbContext = dbContext;

  public void Add(Person person)
  {
    _dbContext.People.Add(person);
  }

  public async Task<Person?> FindByImdbIdAsync(string imdbId, CancellationToken cancellationToken = default)
  {
    return await _dbContext.People
      .Where(p => p.ImdbId == imdbId)
      .FirstOrDefaultAsync(cancellationToken);
  }
}
