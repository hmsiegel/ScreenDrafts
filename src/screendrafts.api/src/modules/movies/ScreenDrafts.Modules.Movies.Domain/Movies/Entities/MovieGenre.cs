namespace ScreenDrafts.Modules.Movies.Domain.Movies.Entities;

public sealed class MovieGenre
{
  private MovieGenre(
    MovieId movieId,
    Guid genreId)
  {
    MovieId = movieId;
    GenreId = genreId;
  }

  private MovieGenre()
  {
  }

  public MovieId MovieId { get; private set; } = default!;

  public Movie Movie { get; private set; } = default!;

  public Guid GenreId { get; private set; } = Guid.Empty;

  public Genre Genre { get; private set; } = default!;

  public static MovieGenre Create(
    MovieId movieId,
    Guid genreId)
  {
    return new MovieGenre(movieId, genreId);
  }
}
