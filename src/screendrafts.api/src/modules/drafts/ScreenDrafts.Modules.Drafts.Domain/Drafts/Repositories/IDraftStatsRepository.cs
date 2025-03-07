namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;

public interface IDraftStatsRepository : IRepository
{
  Task<DrafterDraftStats?> GetByDrafterAndDraftAsync(
    DrafterId drafterId,
    DraftId draftId,
    CancellationToken cancellationToken);

  void Update(DrafterDraftStats drafterDraftStats);

  void Add(DrafterDraftStats drafterDraftStats);

  void Delete(DrafterDraftStats drafterDraftStats);
}
