using Microsoft.EntityFrameworkCore;

namespace ScreenDrafts.Modules.Drafts.Infrastructure.DraftParts;

internal sealed class DraftPartRepository(DraftsDbContext dbContext) : IDraftPartRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public Task<DraftPart?> GetByIdAsync(DraftPartId draftPartId, CancellationToken cancellationToken)
  {
    var draftPart = _dbContext.DraftParts
        .FirstOrDefaultAsync(x => x.Id == draftPartId, cancellationToken);

    return draftPart;
  }

  public void Update(DraftPart draftPart)
  {
    var entry = _dbContext.Entry(draftPart);

    // Only call Update if the entity is not already being tracked
    if (entry.State == EntityState.Detached)
    {
      _dbContext.DraftParts.Update(draftPart);
    }
  }
}
