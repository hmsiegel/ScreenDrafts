namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Repositories;

public interface IDraftPartPredictorRepository
  : IRepository<DraftPartPredictor, DraftPartPredictorId>
{
  Task<IReadOnlyList<DraftPartPredictor>> GetByDraftPartIdAsync(
    DraftPartId draftPartId,
    CancellationToken cancellationToken = default
  );

  Task<DraftPartPredictor?> GetByDraftPartAndContestantAsync(
    DraftPartId draftPartId,
    ContestantId contestantId,
    CancellationToken cancellationToken = default
  );

  void RemoveRange(IEnumerable<DraftPartPredictor> predictors);
}
