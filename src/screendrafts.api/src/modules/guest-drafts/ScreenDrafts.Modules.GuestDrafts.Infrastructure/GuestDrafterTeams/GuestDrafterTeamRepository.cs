namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.GuestDrafterTeams;

internal sealed class GuestDrafterTeamRepository(GuestDraftsDbContext dbContext)
  : IGuestDrafterTeamRepository
{
  private readonly GuestDraftsDbContext _dbContext = dbContext;

  public void Add(GuestDrafterTeam team)
  {
    _dbContext.GuestDrafterTeams.Add(team);
  }

  public void Update(GuestDrafterTeam team)
  {
    _dbContext.GuestDrafterTeams.Update(team);
  }

  public void Delete(GuestDrafterTeam team)
  {
    _dbContext.GuestDrafterTeams.Remove(team);
  }

  public Task<bool> ExistsAsync(GuestDrafterTeamId id, CancellationToken cancellationToken)
  {
    return _dbContext.GuestDrafterTeams.AnyAsync(t => t.Id == id, cancellationToken);
  }

  public Task<GuestDrafterTeam?> GetByIdAsync(
    GuestDrafterTeamId id,
    CancellationToken cancellationToken
  )
  {
    return _dbContext
      .GuestDrafterTeams.Include(t => t.Drafters)
      .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
  }

  public Task<GuestDrafterTeam?> GetByPublicIdAsync(
    string publicId,
    CancellationToken cancellationToken
  )
  {
    return _dbContext
      .GuestDrafterTeams.Include(t => t.Drafters)
      .FirstOrDefaultAsync(t => t.PublicId == publicId, cancellationToken);
  }

  public Task<List<GuestDrafterTeam>> GetAllAsync(CancellationToken cancellationToken)
  {
    return _dbContext.GuestDrafterTeams.ToListAsync(cancellationToken);
  }
}
