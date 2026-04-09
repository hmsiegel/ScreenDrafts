namespace ScreenDrafts.Modules.Drafts.Infrastructure.Predictions.DraftPredictionSets;

internal sealed class DraftPredictionSetRepository(DraftsDbContext dbContext) : IDraftPredictionSetRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(DraftPredictionSet entity)
  {
    _dbContext.DraftPredictionSets.Add(entity);
  }

  public void Delete(DraftPredictionSet entity)
  {
    _dbContext.DraftPredictionSets.Remove(entity);
  }

  public async Task<bool> ExistsAsync(DraftPredictionSetId id, CancellationToken cancellationToken)
  {
    return await _dbContext.DraftPredictionSets.AnyAsync(x => x.Id == id, cancellationToken);
  }

  public async Task<List<DraftPredictionSet>> GetAllAsync(CancellationToken cancellationToken)
  {
    return await _dbContext.DraftPredictionSets.ToListAsync(cancellationToken);
  }

  public async Task<DraftPredictionSet?> GetByContestantAndDraftPartAsync(ContestantId contestantId, DraftPartId draftPartId, CancellationToken cancellationToken = default)
  {
    return await _dbContext.DraftPredictionSets
      .Include(s => s.Entries)
      .FirstOrDefaultAsync(x => x.ContestantId == contestantId && x.DraftPartId == draftPartId, cancellationToken);
  }

  public async Task<IReadOnlyList<DraftPredictionSet>> GetByDraftPartIdAsync(DraftPartId draftPartId, CancellationToken cancellationToken = default)
  {
    return await _dbContext.DraftPredictionSets
      .Include(s => s.Entries)
      .Include(s => s.Surrogates)
      .Include(s => s.Contestant)
      .Where(x => x.DraftPartId == draftPartId)
      .ToListAsync(cancellationToken);
  }

  public async Task<DraftPredictionSet?> GetByIdAsync(DraftPredictionSetId id, CancellationToken cancellationToken)
  {
    return await _dbContext.DraftPredictionSets
      .Include(s => s.Entries)
      .Include(s => s.Surrogates)
      .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
  }

  public async Task<DraftPredictionSet?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken)
  {
    return await _dbContext.DraftPredictionSets
      .Include(s => s.Entries)
      .Include(s => s.Surrogates)
      .FirstOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);
  }

  public void Update(DraftPredictionSet entity)
  {
    _dbContext.DraftPredictionSets.Update(entity);
  }
}
