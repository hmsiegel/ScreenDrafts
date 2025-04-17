
namespace ScreenDrafts.Modules.Movies.Infrastructure.Movies;

internal sealed class MovieRepository(MoviesDbContext context) : IMovieRepository
{
  private readonly MoviesDbContext _context = context;

  public void Add(Movie movie)
  {
    _context.Movies.Add(movie);
  }

  public void AddMovieActor(Movie movie, Person actor)
  {
    _context.MovieActors.Add( MovieActor.Create(movie.Id, actor.Id));
  }

  public void AddMovieDirector(Movie movie, Person director)
  {
    _context.MovieDirectors.Add(MovieDirector.Create(movie.Id, director.Id));
  }

  public void AddMovieGenre(Movie movie, Genre genre)
  {
    _context.MovieGenres.Add(MovieGenre.Create(movie.Id, genre.Id));
  }

  public void AddMovieProducer(Movie movie, Person producer)
  {
    _context.MovieProducers.Add(MovieProducer.Create(movie.Id, producer.Id));
  }

  public void AddMovieProductionCompany(Movie movie, ProductionCompany productionCompany)
  {
    _context.MovieProductionCompanies.Add(MovieProductionCompany.Create(movie.Id, productionCompany.Id));
  }

  public void AddMovieWriter(Movie movie, Person writer)
  {
    _context.MovieWriters.Add(MovieWriter.Create(movie.Id, writer.Id));
  }

  public async Task<bool> ExistsAsync(string imdbId, CancellationToken cancellationToken = default)
  {
    return await _context.Movies.AnyAsync(m => m.ImdbId == imdbId, cancellationToken);
  }

  public async Task<Movie?> FindByImdbIdAsync(string imdbId, CancellationToken cancellationToken = default)
  {
    return await _context.Movies
      .Include(m => m.MovieGenres)
      .Include(m => m.MovieActors)
      .Include(m => m.MovieDirectors)
      .Include(m => m.MovieWriters)
      .Include(m => m.MovieProducers)
      .Include(m => m.MovieProductionCompanies)
      .SingleOrDefaultAsync(m => m.ImdbId == imdbId, cancellationToken);
  }

  public async Task<HashSet<string>> GetExistingMovieImdbsAsync(IEnumerable<string> imdbIds, CancellationToken cancellationToken = default)
  {
    return await _context.Movies
      .Where(m => imdbIds.Contains(m.ImdbId))
      .Select(m => m.ImdbId)
      .ToHashSetAsync(cancellationToken);
  }
}
