
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

  public Task<List<Veto?>> GetVetoesByDraftId(DraftId draftId, CancellationToken cancellationToken)
  {
    var vetoes = _dbContext.Vetoes
      .Where(v => v.Pick.Draft.Id == draftId)
      .ToListAsync(cancellationToken);

    return vetoes!;
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

  public Task<List<VetoOverride?>> GetVetoOverridesByDraftId(DraftId draftId, CancellationToken cancellationToken)
  {
    var vetoOverrides = _dbContext.VetoOverrides
      .Where(vo => vo.Veto.Pick.Draft.Id == draftId)
      .ToListAsync(cancellationToken);

    return vetoOverrides!;
  }
}
