namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Repositories;
public interface IVetoRepository : IRepository
{
  Task<Veto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
