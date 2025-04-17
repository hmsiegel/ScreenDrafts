namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;
public interface IPicksRepository : IRepository
{
  Task<Pick?> GetByIdAsync(PickId id, CancellationToken cancellationToken);

  Task<List<Pick>> GetByDraftIdAsync(DraftId draftId, CancellationToken cancellationToken);

  void Add(Pick pick);

  void Update(Pick pick);
}
