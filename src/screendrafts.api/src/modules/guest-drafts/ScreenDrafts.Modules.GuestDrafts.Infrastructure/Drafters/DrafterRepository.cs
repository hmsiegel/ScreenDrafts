namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.Drafters;

internal sealed class DrafterRepository(GuestDraftsDbContext dbContext) : IDrafterRepository
{
  private readonly GuestDraftsDbContext _dbContext = dbContext;

  public void Add(Drafter drafter)
  {
    _dbContext.Drafters.Add(drafter);
  }

  public void Update(Drafter drafter)
  {
    _dbContext.Drafters.Update(drafter);
  }

  public void Delete(Drafter drafter)
  {
    _dbContext.Drafters.Remove(drafter);
  }

  public Task<bool> ExistsAsync(DrafterId id, CancellationToken cancellationToken)
  {
    return _dbContext.Drafters.AnyAsync(d => d.Id == id, cancellationToken);
  }

  public Task<Drafter?> GetByIdAsync(DrafterId id, CancellationToken cancellationToken)
  {
    return _dbContext.Drafters.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
  }

  public Task<Drafter?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken)
  {
    return _dbContext.Drafters.FirstOrDefaultAsync(d => d.PublicId == publicId, cancellationToken);
  }

  public Task<Drafter?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
  {
    return _dbContext.Drafters.FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);
  }

  public Task<List<Drafter>> GetAllAsync(CancellationToken cancellationToken)
  {
    return _dbContext.Drafters.ToListAsync(cancellationToken);
  }
}
