namespace ScreenDrafts.Modules.Drafts.Infrastructure.People;

internal sealed class HostRepository(DraftsDbContext dbContext) : IHostRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(Host entity)
  {
    _dbContext.Hosts.Add(entity);
  }

  public void Delete(Host entity)
  {
    _dbContext.Hosts.Remove(entity);
  }

  public Task<bool> ExistsAsync(HostId id, CancellationToken cancellationToken)
  {
    return _dbContext.Hosts.AnyAsync(h => h.Id == id, cancellationToken);
  }

  public Task<Host?> GetByIdAsync(HostId id, CancellationToken cancellationToken)
  {
    return _dbContext.Hosts.FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
  }

  public async Task<Host?> GetHostByIdAsync(HostId hostId, CancellationToken cancellationToken)
  {
    var host = await _dbContext.Hosts
      .FirstOrDefaultAsync(h => h.Id == hostId, cancellationToken);

    return host;
  }

  public Task<bool> HostExistsAsync(string personPublicId, CancellationToken cancellationToken)
  {
    return _dbContext.Hosts.AnyAsync(h => h.PublicId == personPublicId, cancellationToken);
  }

  public void Update(Host entity)
  {
    _dbContext.Hosts.Update(entity);
  }
}
