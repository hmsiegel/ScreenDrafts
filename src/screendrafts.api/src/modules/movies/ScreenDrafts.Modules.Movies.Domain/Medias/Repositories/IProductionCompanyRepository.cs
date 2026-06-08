namespace ScreenDrafts.Modules.Movies.Domain.Medias.Repositories;

public interface IProductionCompanyRepository : IRepository
{
  Task<ProductionCompany?> FindByTmdbIdAsync(
    int tmdbId,
    CancellationToken cancellationToken = default
  );
  Task<ProductionCompany?> FindByNameAsync(
    string name,
    CancellationToken cancellationToken = default
  );
  Task<bool> RelationshipExistsAsync(
    Guid mediaId,
    Guid productionCompanyId,
    CancellationToken cancellationToken = default
  );
  void Add(ProductionCompany productionCompany);
  void Attach(ProductionCompany productionCompany);
}
