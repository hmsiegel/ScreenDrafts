namespace ScreenDrafts.Modules.Drafts.Infrastructure.Predictions.PredictionSeasons;

internal sealed class PredictionSeasonRepository(DraftsDbContext dbContext) : IPredictionSeasonRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(PredictionSeason entity)
  {
    _dbContext.Add(entity);
  }

  public void Delete(PredictionSeason entity)
  {
    _dbContext.Remove(entity);
  }

  public Task<bool> ExistsAsync(PredictionSeasonId id, CancellationToken cancellationToken)
  {
    return _dbContext.PredictionSeasons.AnyAsync(x => x.Id == id, cancellationToken);
  }

  public Task<List<PredictionSeason>> GetAllAsync(CancellationToken cancellationToken)
  {
    return _dbContext.PredictionSeasons.ToListAsync(cancellationToken);
  }

  public async Task<PredictionSeason?> GetByIdAsync(PredictionSeasonId id, CancellationToken cancellationToken)
  {
    return await _dbContext.PredictionSeasons
      .Include(s => s.Standings)
        .ThenInclude(st => st.Contestant)
      .Include(s => s.Carryovers)
        .ThenInclude(c => c.Contestant)
      .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
  }

  public async Task<PredictionSeason?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken)
  {
    return await _dbContext.PredictionSeasons
      .Include(s => s.Standings)
        .ThenInclude(st => st.Contestant)
      .Include(s => s.Carryovers)
        .ThenInclude(c => c.Contestant)
      .FirstOrDefaultAsync(x => x.PublicId == publicId, cancellationToken); 
  }

  public async Task<PredictionSeason?> GetCurrentAsync(CancellationToken cancellationToken = default)
  {
    return await _dbContext.PredictionSeasons
      .Include(s => s.Standings)
        .ThenInclude(st => st.Contestant)
      .Include(s => s.Carryovers)
        .ThenInclude(c => c.Contestant)
      .Where(s => !s.IsClosed)
      .OrderByDescending(s => s.Number)
      .FirstOrDefaultAsync(cancellationToken);
  }

  public void Update(PredictionSeason entity)
  {
    _dbContext.PredictionSeasons.Update(entity);
  }
}
