
namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class DraftPartsRepository(DraftsDbContext dbContext) : IDraftPartsRepository
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
