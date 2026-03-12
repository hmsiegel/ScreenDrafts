namespace ScreenDrafts.Modules.Drafts.Domain.DraftPools;

public interface IDraftPoolRepository : IRepository<DraftPool, DraftPoolId>
{
  Task<DraftPool?> GetByDraftIdAsync(DraftId draftId, CancellationToken cancellationToken = default);
}
