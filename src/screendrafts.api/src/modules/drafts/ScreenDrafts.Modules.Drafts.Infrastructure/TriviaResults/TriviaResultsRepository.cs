
namespace ScreenDrafts.Modules.Drafts.Infrastructure.TriviaResults;

internal sealed class TriviaResultsRepository(DraftsDbContext dbContext) : ITriviaResultsRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(TriviaResult entity)
  {
    _dbContext.TriviaResults.Add(entity);
  }

  public void Delete(TriviaResult entity)
  {
    _dbContext.TriviaResults.Remove(entity);
  }

  public void Update(TriviaResult entity)
  {
    _dbContext.TriviaResults.Update(entity);
  }
}
