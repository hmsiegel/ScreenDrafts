namespace ScreenDrafts.Modules.Drafts.Infrastructure.DraftPools;

internal sealed class DraftPoolRepository(DraftsDbContext dbContext) : IDraftPoolRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(DraftPool entity)
  {
    _dbContext.Add(entity);
  }

  public void Delete(DraftPool entity)
  {
    _dbContext.Remove(entity);
  }

  public Task<bool> ExistsAsync(DraftPoolId id, CancellationToken cancellationToken)
  {
    return _dbContext.DraftPools.AnyAsync(x => x.Id == id, cancellationToken);
  }

  public Task<List<DraftPool>> GetAllAsync(CancellationToken cancellationToken)
  {
    return _dbContext.DraftPools.ToListAsync(cancellationToken);
  }

  public Task<DraftPool?> GetByDraftIdAsync(DraftId draftId, CancellationToken cancellationToken = default)
  {
    return _dbContext.DraftPools
      .Include(x => x.TmdbIds)
      .FirstOrDefaultAsync(x => x.DraftId == draftId, cancellationToken);
  }

  public Task<DraftPool?> GetByIdAsync(DraftPoolId id, CancellationToken cancellationToken)
  {
    return _dbContext.DraftPools.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
  }

  public Task<DraftPool?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken)
  {
    return _dbContext.DraftPools.FirstOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);
  }

  public void Update(DraftPool entity)
  {
    var entry = _dbContext.Entry(entity);
    if (entry.State == EntityState.Detached)
    {
      _dbContext.Attach(entity);
      entry.State = EntityState.Modified;
      return;
    }

    // Disable auto-detect so that Entries<T>() doesn't prematurely call DetectChanges.
    // We need to inspect entity states before EF Core interferes.
    _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
    try
    {
      // EF Core DetectChanges does not find new DraftPoolItems because TmdbId
      // (a non-zero user-supplied value) is part of the composite PK and was
      // previously configured with ValueGeneratedOnAdd. Explicitly track new items.
      foreach (var item in entity.TmdbIds)
      {
        var itemEntry = _dbContext.Entry(item);
        if (itemEntry.State == EntityState.Detached)
        {
          itemEntry.State = EntityState.Added;
          itemEntry.Property("DraftPoolId").CurrentValue = entity.Id;
        }
      }

      // EF Core orphan deletion does not reliably generate DELETE for owned entities
      // removed from the collection. Explicitly delete tracked items no longer present.
      var removedItems = _dbContext.ChangeTracker.Entries<DraftPoolItem>()
        .Where(e => e.State == EntityState.Unchanged)
        .Where(e => (DraftPoolId)e.Property("DraftPoolId").CurrentValue! == entity.Id)
        .Select(e => e.Entity)
        .Where(item => !entity.TmdbIds.Contains(item))
        .ToList();
      foreach (var removedItem in removedItems)
      {
        _dbContext.Remove(removedItem);
      }
    }
    finally
    {
      _dbContext.ChangeTracker.AutoDetectChangesEnabled = true;
    }
  }
}
