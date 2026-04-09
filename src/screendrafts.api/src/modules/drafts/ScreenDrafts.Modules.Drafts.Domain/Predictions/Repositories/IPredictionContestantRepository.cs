namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Repositories;

public interface IPredictionContestantRepository : IRepository<PredictionContestant, ContestantId>
{
  Task<PredictionContestant?> GetByPersonIdAsync(PersonId personId, CancellationToken cancellationToken = default);
  Task<IReadOnlyList<PredictionContestant>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}

