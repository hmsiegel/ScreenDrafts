namespace ScreenDrafts.Modules.Drafts.Domain.Hosts;
public interface IHostRepository : IRepository<Host, HostId>
{
  Task<bool> ExistsByPublicIdAsync(string publicId, CancellationToken cancellationToken);
}
