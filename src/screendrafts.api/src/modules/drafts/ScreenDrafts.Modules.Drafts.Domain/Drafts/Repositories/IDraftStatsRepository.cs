namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;

public interface IDraftStatsRepository : IRepository
{
  Task<DrafterDraftStats?> GetByDrafterAndDraftAsync(
    DrafterId drafterId,
    DraftId draftId,
    CancellationToken cancellationToken);
}
