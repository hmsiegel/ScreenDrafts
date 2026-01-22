namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class DraftsRepository(DraftsDbContext dbContext) : IDraftsRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(Draft draft)
  {
    var existingEntity = _dbContext.ChangeTracker.Entries<Draft>()
      .FirstOrDefault(e => e.Entity.Id == draft.Id);

    if (existingEntity is not null)
    {
      return;
    }

    _dbContext.Drafts.Add(draft);
  }

  public void Update(Draft draft)
  {
    _dbContext.Drafts.Update(draft);
  }

  public void Delete(Draft draft)
  {
    _dbContext.Drafts.Remove(draft);
  }

  public void AddCommissionerOverride(CommissionerOverride commissionerOverride)
  {
    _dbContext.CommissionerOverrides.Add(commissionerOverride);
  }

  public async Task<Draft?> GetByIdAsync(DraftId draftId, CancellationToken cancellationToken)
  {
    var draft = await _dbContext.Drafts
      .FirstOrDefaultAsync(d => d.Id == draftId, cancellationToken);

    return draft;
  }

  public async Task<Draft?> GetDraftWithDetailsAsync(DraftId draftId, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public Task<List<CommissionerOverride?>> GetCommissionerOverridesByDraftIdAsync(DraftId draftId, CancellationToken cancellationToken)
  {
    var commissionerOverrides = _dbContext.CommissionerOverrides
      .Where(co => co.Pick.Draft.Id == draftId)
      .ToListAsync(cancellationToken);

    return commissionerOverrides!;
  }

  public async Task<Draft?> GetPreviousDraftAsync(int? episodeNumber, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public async Task<Draft?> GetNextDraftAsync(int? episodeNumber, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public Task<Draft?> GetByDraftPartIdAsync(DraftPartId draftPartId, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public Task<DraftPart?> GetDraftPartByIdAsync(DraftPartId draftPartId, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public Task<List<DraftPart>> GetDraftPartsByDraftIdAsync(DraftId draftId, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public Task<Draft?> GetDraftByDraftPartId(DraftPartId draftPartId, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public Task<Draft?> GetDraftByPublicId(string publicId, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public Task<bool> ExistsAsync(DraftId id, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public async Task<Draft?> GetDraftByPublicIdWithPartsAsNoTrackingAsync(string publicId, CancellationToken cancellationToken)
  {
    return await _dbContext.Drafts
      .AsNoTracking()
      .Include(d => d.Parts)
      .FirstOrDefaultAsync(d => d.PublicId == publicId, cancellationToken);
  }

  public async Task<Draft?> GetDraftByPublicIdWithPartsAsync(string publicId, CancellationToken cancellationToken)
  {
    return await _dbContext.Drafts
      .Include(d => d.Parts)
      .FirstOrDefaultAsync(d => d.PublicId == publicId, cancellationToken);
  }

  public Task<Draft?> GetDraftByPublicIdForUpdateAsync(string publicId, CancellationToken cancellationToken)
  {
    return _dbContext.Drafts
      .Include(d => d.Parts)
      .Include(d => d.DraftCategories)
      .FirstOrDefaultAsync(d => d.PublicId == publicId, cancellationToken);
  }
}
