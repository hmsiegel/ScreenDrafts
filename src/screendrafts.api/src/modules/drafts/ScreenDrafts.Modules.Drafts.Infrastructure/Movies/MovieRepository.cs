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

  public async Task<bool> ExistsAsync(Guid id, CancellationToken ct)
  {
    return await _dbContext.Movies.AnyAsync(m => m.Id == id, ct);
  }

  public async Task<bool> ExistsByImdbIdAsync(string imdbId, CancellationToken ct)
  {
    return await _dbContext.Movies.AnyAsync(m => m.ImdbId == imdbId, ct);
  }

  public async Task<Movie?> GetByIdAsync(Guid id, CancellationToken ct)
  {
    return await _dbContext.Movies
      .Include(m => m.Versions)
      .FirstOrDefaultAsync(m => m.Id == id, ct);
  }

  public void Update(Movie entity)
  {
    _dbContext.Movies.Update(entity);
  }
}
