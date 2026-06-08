namespace ScreenDrafts.Modules.Movies.Infrastructure.ProductionCompanies;

internal sealed class ProductionCompaniesRepository(MoviesDbContext dbContext)
  : MoviesRepositoryBase<ProductionCompany>(dbContext),
    IProductionCompanyRepository
{
  private readonly MoviesDbContext _dbContext = dbContext;

  public void Add(ProductionCompany productionCompany)
  {
    _dbContext.ProductionCompanies.Add(productionCompany);
  }

  public async Task<ProductionCompany?> FindByNameAsync(
    string name,
    CancellationToken cancellationToken = default
  )
  {
    return await _dbContext.ProductionCompanies.FirstOrDefaultAsync(
      pc => pc.Name == name,
      cancellationToken
    );
  }

  public async Task<ProductionCompany?> FindByTmdbIdAsync(
    int tmdbId,
    CancellationToken cancellationToken = default
  )
  {
    return await _dbContext.ProductionCompanies.FirstOrDefaultAsync(
      pc => pc.TmdbId == tmdbId,
      cancellationToken
    );
  }

  public async Task<bool> RelationshipExistsAsync(
    Guid mediaId,
    Guid productionCompanyId,
    CancellationToken cancellationToken = default
  )
  {
    return await _dbContext.MediaProductionCompanies.AnyAsync(
      mpc =>
        mpc.MediaId == MediaId.Create(mediaId) && mpc.ProductionCompanyId == productionCompanyId,
      cancellationToken
    );
  }
}
