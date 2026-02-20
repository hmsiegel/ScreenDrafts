namespace ScreenDrafts.Modules.Drafts.Infrastructure.People;

internal sealed class DrafterTeamRepository(DraftsDbContext dbContext) : IDrafterTeamRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(DrafterTeam entity)
  {
    _dbContext.DrafterTeams.Add(entity);
  }

  public void Delete(DrafterTeam entity)
  {
    _dbContext.DrafterTeams.Remove(entity);
  }

  public Task<bool> ExistsAsync(DrafterTeamId id, CancellationToken cancellationToken)
  {
    return _dbContext.DrafterTeams.AnyAsync(d => d.Id == id, cancellationToken);
  }

  public async Task<List<DrafterTeam>> GetAllAsync(CancellationToken cancellationToken)
  {
    return await _dbContext.DrafterTeams
      .Include(x => x.Drafters)
      .ToListAsync(cancellationToken);
  }

  public async Task<DrafterTeam?> GetByIdAsync(DrafterTeamId drafterTeamId, CancellationToken cancellationToken)
  {
    return await _dbContext.DrafterTeams
      .Include(x => x.Drafters)
      .SingleOrDefaultAsync(d => d.Id == drafterTeamId, cancellationToken);
  }

  public async Task<DrafterTeam?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken)
  {
    return await _dbContext.DrafterTeams
      .Include(x => x.Drafters)
      .SingleOrDefaultAsync(d => d.PublicId == publicId, cancellationToken);
  }

  public void Update(DrafterTeam entity)
  {
    _dbContext.DrafterTeams.Update(entity);
  }
}
