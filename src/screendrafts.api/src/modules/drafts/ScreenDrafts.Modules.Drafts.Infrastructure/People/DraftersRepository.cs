namespace ScreenDrafts.Modules.Drafts.Infrastructure.People;

internal sealed class DraftersRepository(DraftsDbContext dbContext) : IDraftersRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(Drafter drafter)
  {
    _dbContext.Drafters.Add(drafter);
  }

  public void AddDrafterTeam(DrafterTeam drafterTeam)
  {
    _dbContext.DrafterTeams.Add(drafterTeam);
  }

  public async Task<List<Drafter>> GetAll(CancellationToken cancellationToken = default)
  {
    return await _dbContext.Drafters
      .ToListAsync(cancellationToken);

  }

  public async Task<Drafter?> GetByIdAsync(DrafterId drafterId, CancellationToken cancellationToken)
  {
    var drafter = await _dbContext.Drafters
      .SingleOrDefaultAsync(d => d.Id == drafterId, cancellationToken);

    return drafter;
  }

  public async Task<DrafterTeam?> GetByIdAsync(DrafterTeamId drafterTeamId, CancellationToken cancellationToken)
  {
    return await _dbContext.DrafterTeams
      .Include(x => x.Drafters)
      .SingleOrDefaultAsync(d => d.Id == drafterTeamId, cancellationToken);
  }

  public void Update(Drafter drafter)
  {
    _dbContext.Drafters.Update(drafter);
  }

  public void UpdateDrafterTeam(DrafterTeam drafterTeam)
  {
    _dbContext.DrafterTeams.Update(drafterTeam);
  }
}
