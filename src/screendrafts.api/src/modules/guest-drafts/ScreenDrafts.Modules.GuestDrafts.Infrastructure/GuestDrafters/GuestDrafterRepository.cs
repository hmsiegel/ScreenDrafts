namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.GuestDrafters;

internal sealed class GuestDrafterRepository(GuestDraftsDbContext dbContext)
  : IGuestDrafterRepository
{
  private readonly GuestDraftsDbContext _dbContext = dbContext;

  public void Add(GuestDrafter drafter)
  {
    _dbContext.GuestDrafters.Add(drafter);
  }

  public void Update(GuestDrafter drafter)
  {
    _dbContext.GuestDrafters.Update(drafter);
  }

  public void Delete(GuestDrafter drafter)
  {
    _dbContext.GuestDrafters.Remove(drafter);
  }

  public Task<bool> ExistsAsync(GuestDrafterId id, CancellationToken cancellationToken)
  {
    return _dbContext.GuestDrafters.AnyAsync(d => d.Id == id, cancellationToken);
  }

  public Task<GuestDrafter?> GetByIdAsync(GuestDrafterId id, CancellationToken cancellationToken)
  {
    return _dbContext.GuestDrafters.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
  }

  public Task<GuestDrafter?> GetByPublicIdAsync(
    string publicId,
    CancellationToken cancellationToken
  )
  {
    return _dbContext.GuestDrafters.FirstOrDefaultAsync(
      d => d.PublicId == publicId,
      cancellationToken
    );
  }

  public Task<GuestDrafter?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
  {
    return _dbContext.GuestDrafters.FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);
  }

  public Task<List<GuestDrafter>> GetAllAsync(CancellationToken cancellationToken)
  {
    return _dbContext.GuestDrafters.ToListAsync(cancellationToken);
  }
}
