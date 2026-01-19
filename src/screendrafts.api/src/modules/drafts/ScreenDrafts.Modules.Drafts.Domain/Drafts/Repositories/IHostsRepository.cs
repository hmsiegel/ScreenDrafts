namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;
public interface IHostsRepository : IRepository<Host, HostId>
{
  Task<bool> HostExistsAsync(string personPublicId, CancellationToken cancellationToken);
}
