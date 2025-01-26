namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Repositories;
public interface IVetoRepository
{
  Task<Veto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
