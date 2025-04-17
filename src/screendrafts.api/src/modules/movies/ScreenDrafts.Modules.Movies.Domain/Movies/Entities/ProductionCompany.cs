namespace ScreenDrafts.Modules.Movies.Domain.Movies.Entities;

public sealed class ProductionCompany : Entity
{
  private readonly List<MovieProductionCompany> _movieProductionCompanies = [];

  private ProductionCompany(
    string name,
    string imdbId,
    Guid? id = null)
    : base(id ?? Guid.NewGuid())
  {
    Name = Guard.Against.NullOrWhiteSpace(name);
    ImdbId = imdbId;
  }

  private ProductionCompany()
  {
  }

  public IReadOnlyList<MovieProductionCompany> MovieProductionCompanies => _movieProductionCompanies.AsReadOnly();

  public string Name { get; private set; } = default!;

  public string ImdbId { get; private set; } = default!;

  public static ProductionCompany Create(string name, string imdbId, Guid? id = null)
  {
    return new ProductionCompany(
      name: name,
      imdbId: imdbId,
      id: id);
  }
}
