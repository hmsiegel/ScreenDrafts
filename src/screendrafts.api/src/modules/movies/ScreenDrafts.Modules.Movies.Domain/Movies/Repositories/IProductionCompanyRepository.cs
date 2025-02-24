namespace ScreenDrafts.Modules.Movies.Domain.Movies.Repositories;

public interface IProductionCompanyRepository : IRepository
{
  Task<ProductionCompany?> FindByNameAsync(string name, CancellationToken cancellationToken = default);

  Task<ProductionCompany?> FindByImdbIdAsync(string imdbId, CancellationToken cancellationToken = default);

  void Add(ProductionCompany productionCompany);
}
