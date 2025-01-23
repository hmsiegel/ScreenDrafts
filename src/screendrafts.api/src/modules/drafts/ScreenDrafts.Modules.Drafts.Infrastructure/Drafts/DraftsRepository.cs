namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class DraftsRepository(DraftsDbContext dbContext) : IDraftsRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(Draft draft)
  {
    _dbContext.Drafts.Add(draft);
  }
}
