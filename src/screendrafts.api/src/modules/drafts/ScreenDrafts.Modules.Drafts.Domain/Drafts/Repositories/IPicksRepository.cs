namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;
public interface IPicksRepository : IRepository
{
  Task<Pick?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
