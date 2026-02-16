namespace ScreenDrafts.Modules.Drafts.Infrastructure.Movies;

internal sealed class MovieRepository(DraftsDbContext dbContext) : IMovieRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(Movie entity)
  {
    _dbContext.Movies.Add(entity);
  }

  public void Delete(Movie entity)
  {
    _dbContext.Movies.Remove(entity);
  }

  public Task<bool> ExistsAsync(string id, CancellationToken cancellationToken)
  {
    return _dbContext.Movies.AnyAsync(m => m.ImdbId == id, cancellationToken);
  }

  public Task<Movie?> GetByIdAsync(string id, CancellationToken cancellationToken)
  {
    return _dbContext.Movies.FindAsync([id], cancellationToken).AsTask();
  }

  public void Update(Movie entity)
  {
    _dbContext.Movies.Update(entity);
  }
}
