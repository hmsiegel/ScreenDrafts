using ScreenDrafts.Modules.GuestDrafts.Domain.Drafters;

namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.GuestDrafters;

internal sealed class GuestDrafterRepository(GuestDraftsDbContext dbContext) : IDrafterRepository
{
  private readonly GuestDraftsDbContext _dbContext = dbContext;

  public void Add(Drafter drafter)
  {
    _dbContext.GuestDrafters.Add(drafter);
  }

  public void Update(Drafter drafter)
  {
    _dbContext.GuestDrafters.Update(drafter);
  }

  public void Delete(Drafter drafter)
  {
    _dbContext.GuestDrafters.Remove(drafter);
  }

  public Task<bool> ExistsAsync(DrafterId id, CancellationToken cancellationToken)
  {
    return _dbContext.GuestDrafters.AnyAsync(d => d.Id == id, cancellationToken);
  }

  public Task<Drafter?> GetByIdAsync(DrafterId id, CancellationToken cancellationToken)
  {
    return _dbContext.GuestDrafters.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
  }

  public Task<Drafter?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken)
  {
    return _dbContext.GuestDrafters.FirstOrDefaultAsync(
      d => d.PublicId == publicId,
      cancellationToken
    );
  }

  public Task<Drafter?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
  {
    return _dbContext.GuestDrafters.FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);
  }

  public Task<List<Drafter>> GetAllAsync(CancellationToken cancellationToken)
  {
    return _dbContext.GuestDrafters.ToListAsync(cancellationToken);
  }
}
