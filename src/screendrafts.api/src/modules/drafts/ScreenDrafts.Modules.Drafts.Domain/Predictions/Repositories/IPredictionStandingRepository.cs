namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Repositories;

public interface IPredictionStandingRepository : IRepository<PredictionStanding>
{
  Task<PredictionStanding?> GetByContestantAndSeasonAsync(
    ContestantId contestantId,
    PredictionSeasonId seasonId,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyList<PredictionStanding>> GetBySeasonIdAsync(
    PredictionSeasonId seasonId,
    CancellationToken cancellationToken = default);
}

