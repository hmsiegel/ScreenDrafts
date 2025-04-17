namespace ScreenDrafts.Modules.Movies.Application.Movies.Queries.GetMovie;

public sealed record MovieResponse(
  Guid Id,
  string ImdbId,
  string Title,
  string Year,
  string Plot,
  string Image,
  string ReleaseDate,
  string? YouTubeTrailer)
{
  private readonly List<GenreResponse> _genres = [];
  private readonly List<ActorResponse> _actors = [];
  private readonly List<DirectorResponse> _directors = [];
  private readonly List<WriterResponse> _writers = [];
  private readonly List<ProducerResponse> _producers = [];
  private readonly List<ProductionCompanyResponse> _productionCompanies = [];

  public MovieResponse()
    : this(
        Guid.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty)
  {
  }

  public ReadOnlyCollection<GenreResponse> Genres => _genres.AsReadOnly();
  public ReadOnlyCollection<ActorResponse> Actors => _actors.AsReadOnly();
  public ReadOnlyCollection<DirectorResponse> Directors => _directors.AsReadOnly();
  public ReadOnlyCollection<WriterResponse> Writers => _writers.AsReadOnly();
  public ReadOnlyCollection<ProducerResponse> Producers => _producers.AsReadOnly();
  public ReadOnlyCollection<ProductionCompanyResponse> ProductionCompanies => _productionCompanies.AsReadOnly();

  public void AddGenre(GenreResponse genre) => _genres.Add(genre);
  public void AddActor(ActorResponse actor) => _actors.Add(actor);
  public void AddDirector(DirectorResponse director) => _directors.Add(director);
  public void AddWriter(WriterResponse writer) => _writers.Add(writer);
  public void AddProducer(ProducerResponse producer) => _producers.Add(producer);
  public void AddProductionCompany(ProductionCompanyResponse productionCompany) => _productionCompanies.Add(productionCompany);
}
