namespace ScreenDrafts.Modules.Drafts.Domain.Hosts;
public interface IHostRepository : IRepository<Host, HostId>
{
  Task<bool> HostExistsAsync(string personPublicId, CancellationToken cancellationToken);
}
