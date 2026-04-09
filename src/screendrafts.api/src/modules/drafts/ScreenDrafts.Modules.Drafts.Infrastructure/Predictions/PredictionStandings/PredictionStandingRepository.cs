namespace ScreenDrafts.Modules.Drafts.Infrastructure.Predictions.PredictionStandings;

internal sealed class PredictionStandingRepository(DraftsDbContext dbContext)
  : IPredictionStandingRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(PredictionStanding entity)
  {
    _dbContext.PredictionStandings.Add(entity);
  }

  public void Delete(PredictionStanding entity)
  {
    _dbContext.PredictionStandings.Remove(entity);
  }

  public async Task<PredictionStanding?> GetByContestantAndSeasonAsync(ContestantId contestantId, PredictionSeasonId seasonId, CancellationToken cancellationToken = default)
  {
    return await _dbContext.PredictionStandings
      .FirstOrDefaultAsync(
      ps => ps.ContestantId == contestantId && ps.SeasonId == seasonId,
      cancellationToken);
  }

  public async Task<IReadOnlyList<PredictionStanding>> GetBySeasonIdAsync(PredictionSeasonId seasonId, CancellationToken cancellationToken = default)
  {
    return await _dbContext.PredictionStandings
      .Include(s => s.Season)
      .Where(ps => ps.SeasonId == seasonId)
      .OrderByDescending(r => r.Points)
      .ToListAsync(cancellationToken);
  }

  public void Update(PredictionStanding entity)
  {
    _dbContext.PredictionStandings.Update(entity);
  }
}
