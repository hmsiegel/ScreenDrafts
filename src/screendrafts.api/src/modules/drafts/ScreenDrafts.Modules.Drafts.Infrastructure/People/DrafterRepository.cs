namespace ScreenDrafts.Modules.Drafts.Infrastructure.People;

internal sealed class DrafterRepository(DraftsDbContext dbContext) : IDrafterRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(Drafter drafter)
  {
    _dbContext.Drafters.Add(drafter);
  }

  public void Delete(Drafter entity)
  {
    _dbContext.Drafters.Remove(entity);
  }

  public Task<bool> ExistsAsync(DrafterId id, CancellationToken cancellationToken)
  {
    return _dbContext.Drafters.AnyAsync(d => d.Id == id, cancellationToken);
  }

  public Task<bool> ExistsForPersonAsync(string personPublicId, CancellationToken cancellationToken)
  {
    return _dbContext.Drafters.AnyAsync(d => d.PublicId == personPublicId, cancellationToken);
  }

  public async Task<List<Drafter>> GetAllAsync(CancellationToken cancellationToken)
  {
    return await _dbContext.Drafters
      .ToListAsync(cancellationToken);

  }

  public async Task<Drafter?> GetByIdAsync(DrafterId drafterId, CancellationToken cancellationToken)
  {
    var drafter = await _dbContext.Drafters
      .SingleOrDefaultAsync(d => d.Id == drafterId, cancellationToken);

    return drafter;
  }

  public async Task<Drafter?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken)
  {
    var drafter = await _dbContext.Drafters
      .SingleOrDefaultAsync(d => d.PublicId == publicId, cancellationToken);

    return drafter;
  }

  public void Update(Drafter drafter)
  {
    _dbContext.Drafters.Update(drafter);
  }
}
