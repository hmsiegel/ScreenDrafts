namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;
public interface IHostsRepository : IRepository
{
  void AddHost(Host host);

  Task<Host?> GetHostByIdAsync(HostId hostId, CancellationToken cancellationToken);
}
