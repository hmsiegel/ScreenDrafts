namespace ScreenDrafts.Modules.Drafts.Infrastructure.Predictions.PredictionContestants;

internal sealed class PredictionContestantRepository(DraftsDbContext dbContext) : IPredictionContestantRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(PredictionContestant entity)
  {
    _dbContext.PredictionContestants.Add(entity);
  }

  public void Delete(PredictionContestant entity)
  {
    _dbContext.PredictionContestants.Remove(entity);
  }

  public async Task<bool> ExistsAsync(ContestantId id, CancellationToken cancellationToken)
  {
    return await _dbContext.PredictionContestants.AnyAsync(x => x.Id == id, cancellationToken);
  }

  public async Task<IReadOnlyList<PredictionContestant>> GetAllActiveAsync(CancellationToken cancellationToken = default)
  {
    return await _dbContext.PredictionContestants
      .Include(c => c.Person)
      .Where(c => c.IsActive)
      .OrderBy(c => c.DisplayName)
      .ToListAsync(cancellationToken);
  }

  public async Task<List<PredictionContestant>> GetAllAsync(CancellationToken cancellationToken)
  {
    return await _dbContext.PredictionContestants
      .Include(c => c.Person)
      .ToListAsync(cancellationToken);
  }

  public async Task<PredictionContestant?> GetByIdAsync(ContestantId id, CancellationToken cancellationToken)
  {
    return await _dbContext.PredictionContestants
      .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
  }

  public async Task<PredictionContestant?> GetByPersonIdAsync(PersonId personId, CancellationToken cancellationToken = default)
  {
    return await _dbContext.PredictionContestants
      .FirstOrDefaultAsync(x => x.PersonId == personId, cancellationToken);
  }

  public async Task<PredictionContestant?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken)
  {
    return await _dbContext.PredictionContestants
      .FirstOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);
  }

  public void Update(PredictionContestant entity)
  {
    _dbContext.PredictionContestants.Update(entity);
  }
}
