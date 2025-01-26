namespace ScreenDrafts.Modules.Drafts.Infrastructure.Vetoes;

internal sealed class VetoRepository(DraftsDbContext dbContext) : IVetoRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public async Task<Veto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
  {
    return await _dbContext.Vetoes
      .SingleOrDefaultAsync(v => v.Id.Value == id, cancellationToken);
  }
}
