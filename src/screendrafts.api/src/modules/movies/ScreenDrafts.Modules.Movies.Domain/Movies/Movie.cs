using ScreenDrafts.Modules.Movies.Domain.Movies.Errors;

namespace ScreenDrafts.Modules.Movies.Domain.Movies;

public sealed class Movie : AggrgateRoot<MovieId, Guid>
{
  private readonly List<MovieGenre> _movieGenres = [];
  private readonly List<MovieActor> _movieActors = [];
  private readonly List<MovieDirector> _movieDirectors = [];
  private readonly List<MovieWriter> _movieWriters = [];
  private readonly List<MovieProducer> _movieProducers = [];
  private readonly List<MovieProductionCompany> _movieProductionCompanies = [];

  private Movie(
    string title,
    string year,
    string plot,
    string image,
    string releaseDate,
    Uri youtubeTrailerUrl,
    string imdbId,
    MovieId? id = null)
    : base(id ?? MovieId.CreateUnique())
  {
    Title = Guard.Against.NullOrWhiteSpace(title);
    Year = Guard.Against.NullOrWhiteSpace(year);
    Plot = Guard.Against.NullOrWhiteSpace(plot);
    Image = Guard.Against.NullOrWhiteSpace(image);
    ReleaseDate = Guard.Against.NullOrWhiteSpace(releaseDate);
    YoutubeTrailerUrl = Guard.Against.Null(youtubeTrailerUrl);
    ImdbId = Guard.Against.NullOrWhiteSpace(imdbId);
  }

  private Movie()
  {
  }

  public string ImdbId { get; private set; } = default!;

  public string Title { get; private set; } = default!;

  public string Year { get; private set; } = default!;

  public string Plot { get; private set; } = default!;

  public string Image { get; private set; } = default!;

  public string ReleaseDate { get; private set; } = default!;

  public Uri YoutubeTrailerUrl { get; private set; } = default!;

  public IReadOnlyCollection<MovieGenre> MovieGenres => _movieGenres.AsReadOnly();

  public IReadOnlyCollection<MovieActor> MovieActors => _movieActors.AsReadOnly();

  public IReadOnlyCollection<MovieDirector> MovieDirectors => _movieDirectors.AsReadOnly();

  public IReadOnlyCollection<MovieWriter> MovieWriters => _movieWriters.AsReadOnly();

  public IReadOnlyCollection<MovieProducer> MovieProducers => _movieProducers.AsReadOnly();

  public IReadOnlyCollection<MovieProductionCompany> MovieProductionCompanies => _movieProductionCompanies.AsReadOnly();

  public void AddGenre(MovieGenre genre)
  {
    _movieGenres.Add(genre);
  }

  public void AddActor(MovieActor actor)
  {
    _movieActors.Add(actor);
  }

  public void AddDirector(MovieDirector director)
  {
    _movieDirectors.Add(director);
  }

  public void AddWriter(MovieWriter writer)
  {
    _movieWriters.Add(writer);
  }

  public void AddProducer(MovieProducer producer)
  {
    _movieProducers.Add(producer);
  }

  public void AddProductionCompany(MovieProductionCompany productionCompany)
  {
    _movieProductionCompanies.Add(productionCompany);
  }

  public static Result<Movie> Create(
    string title,
    string year,
    string plot,
    string image,
    string releaseDate,
    Uri youtubeTrailerUrl,
    string imdbId,
    MovieId? id = null)
  {
    var movie = new Movie(
      title: title,
      year: year,
      plot: plot,
      image: image,
      releaseDate: releaseDate,
      youtubeTrailerUrl: youtubeTrailerUrl,
      imdbId: imdbId,
      id: id);

    movie.Raise(new MovieCreatedDomainEvent(movie.Id.Value));

    return Result.Success(movie);
  }
}
