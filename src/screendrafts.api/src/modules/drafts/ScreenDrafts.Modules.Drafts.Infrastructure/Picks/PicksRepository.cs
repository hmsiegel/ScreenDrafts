namespace ScreenDrafts.Modules.Drafts.Infrastructure.Picks;

internal sealed class PicksRepository(DraftsDbContext dbContext) : IPicksRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(Pick pick)
  {
    _dbContext.Picks.Add(pick);
  }

  public async Task<Pick?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
  {
    return await _dbContext.Picks
      .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
  }
}
