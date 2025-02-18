namespace ScreenDrafts.Modules.Movies.Domain.Movies.Entities;

public sealed class MovieActor : Entity
{
  private MovieActor(
    Guid id,
    MovieId movieId,
    PersonId actorId)
    : base(id)
  {
    Id = id;
    MovieId = Guard.Against.Null(movieId);
    ActorId = Guard.Against.Null(actorId);
  }

  private MovieActor()
  {
  }

  public MovieId MovieId { get; private set; } = default!;

  public Movie Movie { get; private set; } = default!;

  public PersonId ActorId { get; private set; } = default!;

  public Person Actor { get; private set; } = default!;

  public static MovieActor Create(
    MovieId movieId,
    PersonId actorId,
    Guid? id = null)
  {
    return new MovieActor(
      movieId: movieId,
      actorId: actorId,
      id: id ?? Guid.NewGuid());
  }
}
