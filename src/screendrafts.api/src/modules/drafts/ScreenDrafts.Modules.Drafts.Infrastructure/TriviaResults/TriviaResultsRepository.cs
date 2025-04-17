namespace ScreenDrafts.Modules.Drafts.Infrastructure.TriviaResults;

internal sealed class TriviaResultsRepository(DraftsDbContext dbContext) : ITriviaResultsRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(TriviaResult triviaResult)
  {
    _dbContext.TriviaResults.Add(triviaResult);
  }

  public async Task<TriviaResult?> GetByDrafterIdAsync(DrafterId drafterId, DraftId draftId)
  {
    return await _dbContext.TriviaResults
      .SingleOrDefaultAsync(t => t.Drafter!.Id == drafterId && t.Draft.Id == draftId);
  }
}
