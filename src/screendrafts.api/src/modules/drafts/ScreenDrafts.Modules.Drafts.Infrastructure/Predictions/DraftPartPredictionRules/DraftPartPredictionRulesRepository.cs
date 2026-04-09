namespace ScreenDrafts.Modules.Drafts.Infrastructure.Predictions.DraftPartPredictionRules;

internal sealed class DraftPartPredictionRulesRepository(DraftsDbContext dbContext) : IDraftPartPredictionRulesRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(DraftPartPredictionRule entity)
  {
    _dbContext.DraftPartPredictionRules.Add(entity);
  }

  public void Delete(DraftPartPredictionRule entity)
  {
    _dbContext.DraftPartPredictionRules.Remove(entity);
  }

  public async Task<bool> ExistsAsync(DraftPartPredictionRuleId id, CancellationToken cancellationToken)
  {
    return await _dbContext.DraftPartPredictionRules.AnyAsync(r => r.Id == id, cancellationToken);
  }

  public async Task<List<DraftPartPredictionRule>> GetAllAsync(CancellationToken cancellationToken)
  {
    return await _dbContext.DraftPartPredictionRules.ToListAsync(cancellationToken);
  }

  public async Task<DraftPartPredictionRule?> GetByDraftPartIdAsync(DraftPartId draftPartId, CancellationToken cancellationToken = default)
  {
    return await _dbContext.DraftPartPredictionRules.FirstOrDefaultAsync(r => r.DraftPartId == draftPartId, cancellationToken);
  }

  public async Task<DraftPartPredictionRule?> GetByIdAsync(DraftPartPredictionRuleId id, CancellationToken cancellationToken)
  {
    return await _dbContext.DraftPartPredictionRules.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
  }

  public async Task<DraftPartPredictionRule?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken)
  {
    return await _dbContext.DraftPartPredictionRules.FirstOrDefaultAsync(r => r.PublicId == publicId, cancellationToken);
  }

  public void Update(DraftPartPredictionRule entity)
  {
    _dbContext.DraftPartPredictionRules.Update(entity);
  }
}
