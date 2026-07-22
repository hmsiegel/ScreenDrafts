namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Repositories;

public interface IPredictionSeasonRepository : IRepository<PredictionSeason, PredictionSeasonId>
{
  Task<PredictionSeason?> GetCurrentAsync(CancellationToken cancellationToken = default);

  Task<bool> ExistsByNumberAsync(int number, CancellationToken cancellationToken = default);
}
