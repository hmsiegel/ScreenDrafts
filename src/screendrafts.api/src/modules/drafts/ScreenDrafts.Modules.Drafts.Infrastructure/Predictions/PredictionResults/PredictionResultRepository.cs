namespace ScreenDrafts.Modules.Drafts.Infrastructure.Predictions.PredictionResults;

internal sealed class PredictionResultRepository(DraftsDbContext dbContext) : IPredictionResultRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(PredictionResult entity)
  {
    _dbContext.PredictionResults.Add(entity);
  }

  public void Delete(PredictionResult entity)
  {
    _dbContext.PredictionResults.Remove(entity);
  }

  public async Task<IReadOnlyList<PredictionResult>> GetByDraftPartIdAsync(
    DraftPartId draftPartId,
    CancellationToken cancellationToken = default)
  {
    return await _dbContext.PredictionResults
      .Include(r => r.PredictionSet)
        .ThenInclude(s => s.Contestant)
      .Where(r => r.PredictionSet.DraftPartId == draftPartId)
      .ToListAsync(cancellationToken);
  }

  public async Task<PredictionResult?> GetBySetIdAsync(
    DraftPredictionSetId setId,
    CancellationToken cancellationToken = default)
  {
    return await _dbContext.PredictionResults
      .Include(r => r.PredictionSet)
        .ThenInclude(s => s.Contestant)
      .FirstOrDefaultAsync(r => r.SetId == setId, cancellationToken);
  }

  public void Update(PredictionResult entity)
  {
    _dbContext.PredictionResults.Update(entity);
  }
}
