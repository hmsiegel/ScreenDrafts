namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Repositories;

public interface IPredictionResultRepository : IRepository<PredictionResult>
{
  Task<PredictionResult?> GetBySetIdAsync(DraftPredictionSetId setId, CancellationToken cancellationToken = default);
  Task<IReadOnlyList<PredictionResult>> GetByDraftPartIdAsync(DraftPartId draftPartId, CancellationToken cancellationToken = default);
}

