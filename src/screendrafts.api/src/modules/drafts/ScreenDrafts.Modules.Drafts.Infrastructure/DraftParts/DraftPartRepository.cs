namespace ScreenDrafts.Modules.Drafts.Infrastructure.DraftParts;

internal sealed class DraftPartRepository(DraftsDbContext dbContext) : IDraftPartRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public Task<DraftPart?> GetByIdAsync(DraftPartId draftPartId, CancellationToken cancellationToken)
  {
    var draftPart = _dbContext.DraftParts
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.Id == draftPartId, cancellationToken);

    return draftPart;
  }

  public void Update(DraftPart draftPart)
  {
    _dbContext.DraftParts.Update(draftPart);
  }
}
