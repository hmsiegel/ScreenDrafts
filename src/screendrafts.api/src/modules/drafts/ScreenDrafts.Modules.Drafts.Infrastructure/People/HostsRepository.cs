namespace ScreenDrafts.Modules.Drafts.Infrastructure.People;

internal sealed class HostsRepository(DraftsDbContext dbContext) : IHostsRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void AddHost(Host host)
  {
    _dbContext.Hosts.Add(host);
  }

  public async Task<Host?> GetHostByIdAsync(HostId hostId, CancellationToken cancellationToken)
  {
    var host = await _dbContext.Hosts
      .FirstOrDefaultAsync(h => h.Id == hostId, cancellationToken);

    return host;
  }
}
