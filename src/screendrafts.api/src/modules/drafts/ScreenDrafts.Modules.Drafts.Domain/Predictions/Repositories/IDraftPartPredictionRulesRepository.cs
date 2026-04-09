namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Repositories;

public interface IDraftPartPredictionRulesRepository : IRepository<DraftPartPredictionRule, DraftPartPredictionRuleId>
{
  Task<DraftPartPredictionRule?> GetByDraftPartIdAsync(DraftPartId draftPartId, CancellationToken cancellationToken = default);
}

