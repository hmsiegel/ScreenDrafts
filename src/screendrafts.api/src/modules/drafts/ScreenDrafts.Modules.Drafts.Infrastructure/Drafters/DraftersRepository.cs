namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafters;

internal sealed class DraftersRepository(DraftsDbContext dbContext) : IDraftersRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(Drafter drafter)
  {
    _dbContext.Drafters.Add(drafter);
  }

  public async Task<List<Drafter>> GetAll(CancellationToken cancellationToken = default)
  {
    return await _dbContext.Drafters
      .ToListAsync(cancellationToken);

  }

  public Task<Drafter?> GetByIdAsync(DrafterId drafterId, CancellationToken cancellationToken)
  {
    var drafter = _dbContext.Drafters
      .SingleOrDefaultAsync(d => d.Id == drafterId, cancellationToken);

    return drafter;
  }

  public Task<DrafterTeam?> GetByIdAsync(DrafterTeamId drafterTeamId, CancellationToken cancellationToken)
  {
    return _dbContext.DrafterTeams
      .Include(x => x.Drafters)
      .SingleOrDefaultAsync(d => d.Id == drafterTeamId, cancellationToken);
  }

  public void Update(Drafter drafter)
  {
    _dbContext.Drafters.Update(drafter);
  }
}
