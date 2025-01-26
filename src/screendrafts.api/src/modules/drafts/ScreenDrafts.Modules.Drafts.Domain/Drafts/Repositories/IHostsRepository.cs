namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;
public interface IHostsRepository
{
  void AddHost(Host host);

  Task<Host?> GetHostByIdAsync(Guid hostId, CancellationToken cancellationToken);
}
