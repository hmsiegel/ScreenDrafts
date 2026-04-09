namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Repositories;

public interface IDraftPredictionSetRepository : IRepository<DraftPredictionSet, DraftPredictionSetId>
{
  Task<IReadOnlyList<DraftPredictionSet>> GetByDraftPartIdAsync(DraftPartId draftPartId, CancellationToken cancellationToken = default);
  Task<DraftPredictionSet?> GetByContestantAndDraftPartAsync(ContestantId contestantId, DraftPartId draftPartId, CancellationToken cancellationToken = default);
}

