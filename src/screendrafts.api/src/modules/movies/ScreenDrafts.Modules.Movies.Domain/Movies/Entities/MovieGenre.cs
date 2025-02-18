namespace ScreenDrafts.Modules.Movies.Domain.Movies.Entities;

public sealed class MovieGenre
{
  public MovieId MovieId { get; private set; } = default!;

  public Movie Movie { get; private set; } = default!;

  public Guid GenreId { get; private set; } = Guid.Empty;

  public Genre Genre { get; private set; } = default!;
}
