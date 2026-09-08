using ScreenDrafts.Modules.GuestDrafts.Domain.DrafterTeams;

namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.GuestDrafterTeams;

internal sealed class GuestDrafterTeamRepository(GuestDraftsDbContext dbContext)
  : IDrafterTeamRepository
{
  private readonly GuestDraftsDbContext _dbContext = dbContext;

  public void Add(DrafterTeam team)
  {
    _dbContext.GuestDrafterTeams.Add(team);
  }

  public void Update(DrafterTeam team)
  {
    _dbContext.GuestDrafterTeams.Update(team);
  }

  public void Delete(DrafterTeam team)
  {
    _dbContext.GuestDrafterTeams.Remove(team);
  }

  public Task<bool> ExistsAsync(DrafterTeamId id, CancellationToken cancellationToken)
  {
    return _dbContext.GuestDrafterTeams.AnyAsync(t => t.Id == id, cancellationToken);
  }

  public Task<DrafterTeam?> GetByIdAsync(DrafterTeamId id, CancellationToken cancellationToken)
  {
    return _dbContext
      .GuestDrafterTeams.Include(t => t.Drafters)
      .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
  }

  public Task<DrafterTeam?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken)
  {
    return _dbContext
      .GuestDrafterTeams.Include(t => t.Drafters)
      .FirstOrDefaultAsync(t => t.PublicId == publicId, cancellationToken);
  }

  public Task<List<DrafterTeam>> GetAllAsync(CancellationToken cancellationToken)
  {
    return _dbContext.GuestDrafterTeams.ToListAsync(cancellationToken);
  }
}
