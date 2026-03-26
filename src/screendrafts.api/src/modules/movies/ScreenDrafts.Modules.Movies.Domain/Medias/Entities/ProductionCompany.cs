namespace ScreenDrafts.Modules.Movies.Domain.Medias.Entities;

public sealed class ProductionCompany : Entity
{
  private readonly List<MediaProductionCompany> _mediaProductionCompanies = [];

  private ProductionCompany(
    string name,
    string imdbId,
    int tmdbId,
    Guid? id = null)
    : base(id ?? Guid.NewGuid())
  {
    Name = Guard.Against.NullOrWhiteSpace(name);
    ImdbId = imdbId;
    TmdbId = tmdbId;
  }

  private ProductionCompany()
  {
  }

  public IReadOnlyList<MediaProductionCompany> MediaProductionCompanies => _mediaProductionCompanies.AsReadOnly();

  public string Name { get; private set; } = default!;
  public string ImdbId { get; private set; } = default!;
  public int TmdbId { get; private set; }

  public static ProductionCompany Create(string name, string imdbId, int tmdbId, Guid? id = null)
  {
    return new ProductionCompany(
      name: name,
      imdbId: imdbId,
      tmdbId: tmdbId,
      id: id);
  }
}
