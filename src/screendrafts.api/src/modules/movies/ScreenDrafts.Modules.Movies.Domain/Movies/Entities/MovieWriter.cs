namespace ScreenDrafts.Modules.Movies.Domain.Movies.Entities;

public sealed class MovieWriter : Entity
{
  private MovieWriter(
    Guid id,
    MovieId movieId,
    PersonId actorId)
    : base(id)
  {
    Id = id;
    MovieId = Guard.Against.Null(movieId);
    WriterId = Guard.Against.Null(actorId);
  }

  private MovieWriter()
  {
  }

  public MovieId MovieId { get; private set; } = default!;

  public Movie Movie { get; private set; } = default!;

  public PersonId WriterId { get; private set; } = default!;

  public Person Writer { get; private set; } = default!;

  public static MovieWriter Create(
    MovieId movieId,
    PersonId actorId,
    Guid? id = null)
  {
    return new MovieWriter(
      movieId: movieId,
      actorId: actorId,
      id: id ?? Guid.NewGuid());
  }
}
