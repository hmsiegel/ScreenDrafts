
namespace ScreenDrafts.Modules.Drafts.Infrastructure.Picks;

internal sealed class PicksRepository(DraftsDbContext dbContext) : IPicksRepository
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

  public Task<List<Pick>> GetByDraftIdAsync(DraftId draftId, CancellationToken cancellationToken)
  {
    var query = _dbContext.Picks
      .Where(p => p.DraftId == draftId)
      .Include(p => p.Movie)
      .OrderBy(p => p.Position);

    return query.ToListAsync(cancellationToken);
  }

  public async Task<Pick?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
  {
    return await _dbContext.Picks
      .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
  }
}
