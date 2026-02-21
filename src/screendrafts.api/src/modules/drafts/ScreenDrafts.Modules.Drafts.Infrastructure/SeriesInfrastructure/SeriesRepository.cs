namespace ScreenDrafts.Modules.Drafts.Infrastructure.SeriesInfrastructure;

internal sealed class SeriesRepository(DraftsDbContext dbContext) : ISeriesRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(Series series)
  {
    _dbContext.Series.Add(series);
  }

  public void Delete(Series series)
  {
    _dbContext.Series.Remove(series);
  }

  public void Update(Series series)
  {
    _dbContext.Series.Update(series);
  }

  public Task<bool> ExistsAsync(SeriesId id, CancellationToken cancellationToken)
  {
    return _dbContext.Series.AnyAsync(s => s.Id == id, cancellationToken);
  }

  public Task<Series?> GetByIdAsync(SeriesId id, CancellationToken cancellationToken)
  {
    return _dbContext.Series.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
  }

  public Task<Series?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken)
  {
    return _dbContext.Series.FirstOrDefaultAsync(s => s.PublicId == publicId, cancellationToken);
  }

  public Task<bool> ExistsByPublicIdAsync(string? seriesPublicId, CancellationToken cancellationToken)
  {
    return _dbContext.Series.AnyAsync(s => s.PublicId == seriesPublicId, cancellationToken);
  }

  public Task<List<Series>> GetAllAsync(CancellationToken cancellationToken)
  {
    return _dbContext.Series.ToListAsync(cancellationToken);
  }
}
