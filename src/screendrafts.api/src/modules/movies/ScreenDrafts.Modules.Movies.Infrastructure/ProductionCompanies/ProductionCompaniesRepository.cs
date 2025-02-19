namespace ScreenDrafts.Modules.Movies.Infrastructure.ProductionCompanies;

internal sealed class ProductionCompaniesRepository(MoviesDbContext dbContext)
  : IProductionCompanyRepository
{
  private readonly MoviesDbContext _dbContext = dbContext;

  public void Add(ProductionCompany productionCompany)
  {
    _dbContext.ProductionCompanies.Add(productionCompany);
  }

  public async Task<ProductionCompany?> FindByImdbIdAsync(string imdbId, CancellationToken cancellationToken = default)
  {
    return await _dbContext.ProductionCompanies
      .Where(pc => pc.ImdbId == imdbId)
      .SingleOrDefaultAsync(cancellationToken);
  }

  public async Task<ProductionCompany?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
  {
    return await _dbContext.ProductionCompanies
      .Where(pc => pc.Name == name)
      .SingleOrDefaultAsync(cancellationToken);
  }
}
