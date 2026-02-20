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

  public void Update(Host entity)
  {
    _dbContext.Hosts.Update(entity);
  }

  public Task<bool> ExistsAsync(HostId id, CancellationToken cancellationToken)
  {
    return _dbContext.Hosts.AnyAsync(h => h.Id == id, cancellationToken);
  }

  public Task<Host?> GetByIdAsync(HostId id, CancellationToken cancellationToken)
  {
    return _dbContext.Hosts.FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
  }

  public async Task<bool> ExistsByPublicIdAsync(string publicId, CancellationToken cancellationToken)
  {
    return await _dbContext.Hosts.AnyAsync(h => h.PublicId == publicId, cancellationToken);
  }

  public async Task<Host?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken)
  {
    return await _dbContext.Hosts.FirstOrDefaultAsync(h => h.PublicId == publicId, cancellationToken);
  }

  public async Task<List<Host>> GetAllAsync(CancellationToken cancellationToken)
  {
    return await _dbContext.Hosts.ToListAsync(cancellationToken);
  }
}
