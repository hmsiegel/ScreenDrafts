namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Repositories;
public interface IPickRepository : IRepository<Pick>
{
  Task<Pick?> GetByDraftPartIdAndPlayOrderAsync(DraftPartId id, int playOrder, CancellationToken cancellationToken);
  Task<Pick?> GetByDraftPartIdAndPlayOrderAsync(DraftPartId id, int playOrder, SubDraftId subDraftId, CancellationToken cancellationToken) ;
  Task<Pick?> GetByIdAsync(PickId id, CancellationToken cancellationToken);
}
