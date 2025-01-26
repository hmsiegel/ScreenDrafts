namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafters;

internal sealed class DraftersRepository(DraftsDbContext dbContext) : IDraftersRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(Drafter drafter)
  {
    _dbContext.Drafters.Add(drafter);
  }

  public Task<Drafter?> GetByIdAsync(Guid drafterId, CancellationToken cancellationToken)
  {
    var drafter = _dbContext.Drafters
      .SingleOrDefaultAsync(d => d.Id.Value == drafterId, cancellationToken);

    return drafter;
  }
}
