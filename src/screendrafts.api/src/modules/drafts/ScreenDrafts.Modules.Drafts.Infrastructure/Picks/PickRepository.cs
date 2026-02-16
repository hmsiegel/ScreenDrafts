
namespace ScreenDrafts.Modules.Drafts.Infrastructure.Picks;

internal sealed class PickRepository(DraftsDbContext dbContext) : IPickRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(Pick pick)
  {
    _dbContext.Picks.Add(pick);
  }

  public void Update(Pick pick)
  {
    _dbContext.Picks.Update(pick);
  }

  public async Task<Pick?> GetByIdAsync(PickId id, CancellationToken cancellationToken)
  {
    return await _dbContext.Picks
      .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
  }

  public void Delete(Pick entity)
  {
    _dbContext.Picks.Remove(entity);
  }
}
