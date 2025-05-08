namespace ScreenDrafts.Modules.Drafts.Infrastructure.Blessings;

internal sealed class VetoRepository(DraftsDbContext dbContext) : IVetoRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public async Task<Veto?> GetByIdAsync(VetoId id, CancellationToken cancellationToken)
  {
    return await _dbContext.Vetoes
      .SingleOrDefaultAsync(v => v.Id == id, cancellationToken);
  }

  public async Task<Veto?> GetByPickAsync(PickId pickId, CancellationToken cancellationToken)
  {
    return await _dbContext.Vetoes
      .SingleOrDefaultAsync(v => v.Pick.Id == pickId, cancellationToken);
  }

  public async Task<VetoOverride?> GetVetoOverrideByIdAsync(VetoOverrideId id, CancellationToken cancellationToken)
  {
    return await _dbContext.VetoOverrides
      .SingleOrDefaultAsync(vo => vo.Id == id, cancellationToken);
  }

  public async Task<VetoOverride?> GetVetoOverrideByVetoIdAsync(VetoId vetoId, CancellationToken cancellationToken)
  {
    return await _dbContext.VetoOverrides
      .SingleOrDefaultAsync(vo => vo.Veto.Id == vetoId, cancellationToken);
  }
}
