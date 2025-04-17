namespace ScreenDrafts.Modules.Movies.Infrastructure.ProductionCompanies;

internal sealed class ProductionCompaniesRepository(MoviesDbContext dbContext)
  : MoviesRepositoryBase<ProductionCompany>(dbContext), IProductionCompanyRepository
{
  private readonly MoviesDbContext _dbContext = dbContext;

  public void Add(ProductionCompany productionCompany)
  {
    if (_dbContext.Entry(productionCompany).State == EntityState.Detached)
    {
      _dbContext.ProductionCompanies.Attach(productionCompany);
    }
    _dbContext.ProductionCompanies.Add(productionCompany);
  }

  public async Task<ProductionCompany?> FindByImdbIdAsync(string imdbId, CancellationToken cancellationToken = default)
  {
    return await _dbContext.ProductionCompanies
      .FirstOrDefaultAsync(pc => pc.ImdbId == imdbId, cancellationToken);
  }

  public async Task<ProductionCompany?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
  {
    return await _dbContext.ProductionCompanies
      .FirstOrDefaultAsync(pc => pc.Name == name, cancellationToken);
  }

  public ProductionCompany? FindExistingEntity(string imdbId, CancellationToken cancellationToken = default)
  {
    var entity = _dbContext.ChangeTracker.Entries<ProductionCompany>()
      .FirstOrDefault(pc => pc.Entity.ImdbId == imdbId)?.Entity;
    return entity;
  }

  public async Task<bool> RelationshipExistsAsync(Guid movieId, Guid productionCompanyId, CancellationToken cancellationToken = default)
  {
    return await _dbContext.MovieProductionCompanies
      .AnyAsync(mpc => mpc.MovieId == MovieId.Create(movieId) &&
        mpc.ProductionCompanyId == productionCompanyId, cancellationToken);
  }
}
