namespace ScreenDrafts.Modules.Drafts.Infrastructure.DrafterStats;

internal sealed class DraftStatsRepository(DraftsDbContext dbContext) : IDraftStatsRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(DrafterDraftStats drafterDraftStats)
  {
    _dbContext.DrafterDraftStats.Add(drafterDraftStats);
  }

  public void Delete(DrafterDraftStats drafterDraftStats)
  {
     _dbContext.DrafterDraftStats.Remove(drafterDraftStats);
  }

  public async Task<DrafterDraftStats?> GetByDrafterAndDraftAsync(DrafterId drafterId, DraftId draftId, CancellationToken cancellationToken)
  {
    return await _dbContext.DrafterDraftStats.Where(x => x.DraftId == draftId && x.DrafterId == drafterId).FirstOrDefaultAsync(cancellationToken);
  }

  public void Update(DrafterDraftStats drafterDraftStats)
  {
    _dbContext.DrafterDraftStats.Update(drafterDraftStats);
  }
}
