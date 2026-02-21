namespace ScreenDrafts.Modules.Drafts.Domain.Hosts;
public interface IHostRepository : IRepository<Host, HostId>
{
  Task<bool> ExistsByPersonPublicId(string personPublicId, CancellationToken cancellationToken);
  Task<bool> ExistsByPublicIdAsync(string publicId, CancellationToken cancellationToken);
}
