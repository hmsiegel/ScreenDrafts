namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;
public interface IPicksRepository : IRepository
{
  Task<Pick?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

  Task<List<Pick>> GetByDraftIdAsync(DraftId draftId, CancellationToken cancellationToken);

  void Add(Pick pick);

  void Update(Pick pick);
}
