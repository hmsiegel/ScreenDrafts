namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Repositories;
public interface IPickRepository : IRepository<Pick>
{
  Task<Pick?> GetByIdAsync(PickId id, CancellationToken cancellationToken);
}
