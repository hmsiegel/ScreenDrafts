namespace ScreenDrafts.Modules.Drafts.Infrastructure.Series;

using Series = Domain.Drafts.Entities.Series;

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

}
