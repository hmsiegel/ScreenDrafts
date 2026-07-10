namespace ScreenDrafts.Modules.Drafts.Infrastructure.Predictions;

internal sealed class DraftPartPredictorRepository(DraftsDbContext dbContext)
  : IDraftPartPredictorRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(DraftPartPredictor entity)
  {
    _dbContext.Add(entity);
  }

  public void Delete(DraftPartPredictor entity)
  {
    _dbContext.Remove(entity);
  }

  public Task<bool> ExistsAsync(DraftPartPredictorId id, CancellationToken cancellationToken)
  {
    return _dbContext.DraftPartPredictors.AnyAsync(x => x.Id == id, cancellationToken);
  }

  public Task<List<DraftPartPredictor>> GetAllAsync(CancellationToken cancellationToken)
  {
    return _dbContext.DraftPartPredictors.ToListAsync(cancellationToken);
  }

  public async Task<DraftPartPredictor?> GetByDraftPartAndContestantAsync(
    DraftPartId draftPartId,
    ContestantId contestantId,
    CancellationToken cancellationToken = default
  )
  {
    return await _dbContext.DraftPartPredictors.FirstOrDefaultAsync(
      x => x.DraftPartId == draftPartId && x.ContestantId == contestantId,
      cancellationToken
    );
  }

  public async Task<IReadOnlyList<DraftPartPredictor>> GetByDraftPartIdAsync(
    DraftPartId draftPartId,
    CancellationToken cancellationToken = default
  )
  {
    return await _dbContext
      .DraftPartPredictors.Where(x => x.DraftPartId == draftPartId)
      .ToListAsync(cancellationToken);
  }

  public async Task<DraftPartPredictor?> GetByIdAsync(
    DraftPartPredictorId id,
    CancellationToken cancellationToken
  )
  {
    return await _dbContext.DraftPartPredictors.FirstOrDefaultAsync(
      x => x.Id == id,
      cancellationToken
    );
  }

  public async Task<DraftPartPredictor?> GetByPublicIdAsync(
    string publicId,
    CancellationToken cancellationToken
  )
  {
    return await _dbContext.DraftPartPredictors.FirstOrDefaultAsync(
      x => x.PublicId == publicId,
      cancellationToken
    );
  }

  public void RemoveRange(IEnumerable<DraftPartPredictor> predictors)
  {
    _dbContext.DraftPartPredictors.RemoveRange(predictors);
  }

  public void Update(DraftPartPredictor entity)
  {
    _dbContext.DraftPartPredictors.Update(entity);
  }
}
