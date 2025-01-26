namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;
public interface IPicksRepository
{
  Task<Pick?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
