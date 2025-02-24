namespace ScreenDrafts.Modules.Movies.Domain.Movies.Entities;

public sealed class MovieDirector : Entity
{
  private MovieDirector(
    Guid id,
    MovieId movieId,
    PersonId actorId)
    : base(id)
  {
    Id = id;
    MovieId = Guard.Against.Null(movieId);
    DirectorId = Guard.Against.Null(actorId);
  }

  private MovieDirector()
  {
  }

  public MovieId MovieId { get; private set; } = default!;

  public Movie Movie { get; private set; } = default!;

  public PersonId DirectorId { get; private set; } = default!;

  public Person Director { get; private set; } = default!;

  public static MovieDirector Create(
    MovieId movieId,
    PersonId actorId,
    Guid? id = null)
  {
    return new MovieDirector(
      movieId: movieId,
      actorId: actorId,
      id: id ?? Guid.NewGuid());
  }
}
