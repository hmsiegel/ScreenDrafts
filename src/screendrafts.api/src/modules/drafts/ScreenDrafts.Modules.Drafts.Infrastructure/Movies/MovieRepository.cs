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

  public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
  {
    return await _dbContext.Movies.AnyAsync(m => m.Id == id, cancellationToken);
  }

  public async Task<bool> ExistsByIgdbIdAsync(int igdbId, CancellationToken cancellationToken)
  {
    return await _dbContext.Movies.AnyAsync(m => m.IgdbId == igdbId, cancellationToken);
  }

  public async Task<bool> ExistsByImdbIdAsync(string imdbId, CancellationToken cancellationToken)
  {
    return await _dbContext.Movies.AnyAsync(m => m.ImdbId == imdbId, cancellationToken);
  }

  public Task<bool> ExistsByPublicIdAsync(string publicId, CancellationToken cancellationToken)
  {
    return _dbContext.Movies.AnyAsync(m => m.PublicId == publicId, cancellationToken);
  }

  public async Task<bool> ExistsByTmdbIdAsync(int tmdbId, CancellationToken cancellationToken)
  {
    return await _dbContext.Movies.AnyAsync(m => m.TmdbId == tmdbId, cancellationToken);
  }

  public Task<List<Movie>> GetAllAsync(CancellationToken cancellationToken)
  {
    return _dbContext.Movies
      .Include(m => m.Versions)
      .ToListAsync(cancellationToken);
  }

  public async Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
  {
    return await _dbContext.Movies
      .Include(m => m.Versions)
      .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
  }

  public async Task<Movie?> GetByIgdbIdAsync(int igdbId, CancellationToken cancellationToken)
  {
    return await _dbContext.Movies
      .Include(m => m.Versions)
      .FirstOrDefaultAsync(m => m.IgdbId == igdbId, cancellationToken);
  }

  public async Task<Movie?> GetByImdbIdAsync(string imdbId, CancellationToken cancellationToken)
  {
    return await _dbContext.Movies
      .Include(m => m.Versions)
      .FirstOrDefaultAsync(m => m.ImdbId == imdbId, cancellationToken);
  }

  public async Task<Movie?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken)
  {
    return await _dbContext.Movies
      .Include(m => m.Versions)
      .FirstOrDefaultAsync(m => m.PublicId == publicId, cancellationToken);
  }

  public async Task<Movie?> GetByTmdbIdAsync(int tmdbId, CancellationToken cancellationToken)
  {
    return await _dbContext.Movies
      .Include(m => m.Versions)
      .FirstOrDefaultAsync(m => m.TmdbId == tmdbId, cancellationToken);
  }

  public async Task<HashSet<int>> GetExistingIgdbIdsAsync(IReadOnlyList<int> validIgdbIds, CancellationToken cancellationToken)
  {
    var existingIds = await _dbContext.Movies
      .Where(m => m.IgdbId != null && validIgdbIds.Contains(m.IgdbId!.Value))
      .Select(m => m.IgdbId!.Value)
      .ToListAsync(cancellationToken);

    return existingIds.ToHashSet();
  }

  public async Task<HashSet<int>> GetExistingTmdbIdsAsync(IReadOnlyList<int> validTmdbIds, CancellationToken cancellationToken)
  {
    var existingIds = await _dbContext.Movies
      .Where(m => m.TmdbId != null && validTmdbIds.Contains(m.TmdbId!.Value))
      .Select(m => m.TmdbId!.Value)
      .ToListAsync(cancellationToken);

    return [..existingIds];

  }

  public void Update(Movie entity)
  {
    _dbContext.Movies.Update(entity);
  }
}
