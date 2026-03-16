namespace ScreenDrafts.Modules.Drafts.Infrastructure.CandidateListEntries;

internal sealed class CandidateListEntryRepository(DraftsDbContext dbContext) : ICandidateListRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(CandidateListEntry entity)
  {
    _dbContext.Add(entity);
  }

  public async Task AddRange(IReadOnlyList<CandidateListEntry> entries)
  {
    await _dbContext.AddRangeAsync(entries);
  }

  public void Delete(CandidateListEntry entity)
  {
    _dbContext.Remove(entity);
  }

  public async Task<bool> ExistsAsync(CandidateListEntryId id, CancellationToken cancellationToken)
  {
    return await _dbContext.CandidateListEntries
      .AnyAsync(e => e.Id == id, cancellationToken);
  }

  public async Task<CandidateListEntry?> FindByTmdbIdAsync(DraftPartId draftPartId, int tmdbId, CancellationToken cancellationToken = default)
  {
    return await _dbContext.CandidateListEntries
      .FirstOrDefaultAsync(e => e.DraftPartId == draftPartId && e.TmdbId == tmdbId, cancellationToken);
  }

  public async Task<Guid?> FindMovieByTmdbIdAsync(int tmdbId, CancellationToken cancellationToken = default)
  {
    return await _dbContext.Movies
      .Where(e => e.TmdbId == tmdbId )
      .Select(e => (Guid?)e.Id)
      .FirstOrDefaultAsync(cancellationToken);
  }

  public async Task<Dictionary<int, Guid>> FindMoviesByTmdbIdsAsync(IReadOnlyList<int> tmdbIds, CancellationToken cancellationToken = default)
  {
    return await _dbContext.Movies
      .Where(m => m.TmdbId != null && tmdbIds.Contains(m.TmdbId!.Value))
      .ToDictionaryAsync(m => m.TmdbId!.Value, m => m.Id, cancellationToken);
  }

  public async Task<List<CandidateListEntry>> GetAllAsync(CancellationToken cancellationToken)
  {
    return await _dbContext.CandidateListEntries.ToListAsync(cancellationToken);
  }

  public async Task<CandidateListEntry?> GetByIdAsync(CandidateListEntryId id, CancellationToken cancellationToken)
  {
    return await _dbContext.CandidateListEntries
      .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
  }

  public Task<CandidateListEntry?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public async Task<HashSet<int>> GetExistingTmdbIdsAsync(DraftPartId draftPartId, CancellationToken cancellationToken = default)
  {
    var ids = await _dbContext.CandidateListEntries
      .Where(e => e.DraftPartId == draftPartId)
      .Select(e => e.TmdbId)
      .ToListAsync(cancellationToken);

    return [.. ids];
  }

  public async Task<List<CandidateListEntry>> GetPendingEntriesByTmdbIdAsync(int tmdbId, CancellationToken cancellationToken = default)
  {
    return await _dbContext.CandidateListEntries
      .Where(e => e.TmdbId == tmdbId && e.IsPending)
      .ToListAsync(cancellationToken);
  }

  public void Update(CandidateListEntry entity)
  {
    _dbContext.Update(entity);
  }

  public void UpdateRange(IReadOnlyList<CandidateListEntry> entries)
  {
    _dbContext.UpdateRange(entries);
  }
}
