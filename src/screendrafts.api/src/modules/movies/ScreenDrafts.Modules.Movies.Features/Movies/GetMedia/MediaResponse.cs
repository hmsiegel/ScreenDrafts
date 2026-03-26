namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMedia;

public sealed record MediaResponse
{
  private readonly List<GenreResponse> _genres = [];
  private readonly List<ActorResponse> _actors = [];
  private readonly List<DirectorResponse> _directors = [];
  private readonly List<WriterResponse> _writers = [];
  private readonly List<ProducerResponse> _producers = [];
  private readonly List<ProductionCompanyResponse> _productionCompanies = [];

  public Guid Id { get; init; }
  public string PublicId { get; init; } = default!;
  public string? ImdbId { get; init; }
  public int? TmdbId { get; init; }
  public int? IgdbId { get; init; }
  public string Title { get; init; } = default!;
  public string Year { get; init; } = default!;
  public string? Plot { get; init; }
  public string? Image { get; init; }
  public string? ReleaseDate { get; init; }
  public string? YouTubeTrailer { get; init; }
  public MediaType MediaType { get; init; } = default!;
  public int? TvSeriesTmdbId { get; init; }
  public int? SeasonNumber { get; init; }
  public int? EpisodeNumber { get; init; }


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
