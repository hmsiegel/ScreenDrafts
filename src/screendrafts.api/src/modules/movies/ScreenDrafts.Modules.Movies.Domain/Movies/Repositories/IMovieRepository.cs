namespace ScreenDrafts.Modules.Movies.Domain.Movies.Repositories;

public interface IMovieRepository : IRepository
{
  void Add(Movie movie);

  Task<bool> ExistsAsync(string imdbId, CancellationToken cancellationToken = default);

  Task<Movie?> FindByImdbIdAsync(string imdbId, CancellationToken cancellationToken = default);

  void AddMovieActor(Movie movie, Person actor);

  void AddMovieDirector(Movie movie, Person director);

  void AddMovieWriter(Movie movie, Person writer);

  void AddMovieProducer(Movie movie, Person producer);

  void AddMovieGenre(Movie movie, Genre genre);

  void AddMovieProductionCompany(Movie movie, ProductionCompany productionCompany);
}
