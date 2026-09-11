namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.DrafterTeams;

internal sealed class DrafterTeamRepository(GuestDraftsDbContext dbContext) : IDrafterTeamRepository
{
  private readonly GuestDraftsDbContext _dbContext = dbContext;

  public void Add(DrafterTeam team)
  {
    _dbContext.DrafterTeams.Add(team);
  }

  public void Update(DrafterTeam team)
  {
    _dbContext.DrafterTeams.Update(team);
  }

  public void Delete(DrafterTeam team)
  {
    _dbContext.DrafterTeams.Remove(team);
  }

  public Task<bool> ExistsAsync(DrafterTeamId id, CancellationToken cancellationToken)
  {
    return _dbContext.DrafterTeams.AnyAsync(t => t.Id == id, cancellationToken);
  }

  public Task<DrafterTeam?> GetByIdAsync(DrafterTeamId id, CancellationToken cancellationToken)
  {
    return _dbContext
      .DrafterTeams.Include(t => t.Drafters)
      .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
  }

  public Task<DrafterTeam?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken)
  {
    return _dbContext
      .DrafterTeams.Include(t => t.Drafters)
      .FirstOrDefaultAsync(t => t.PublicId == publicId, cancellationToken);
  }

  public Task<List<DrafterTeam>> GetAllAsync(CancellationToken cancellationToken)
  {
    return _dbContext.DrafterTeams.ToListAsync(cancellationToken);
  }
}
