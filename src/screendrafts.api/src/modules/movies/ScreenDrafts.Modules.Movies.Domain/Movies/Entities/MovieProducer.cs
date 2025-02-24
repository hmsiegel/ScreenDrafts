namespace ScreenDrafts.Modules.Movies.Domain.Movies.Entities;

public sealed class MovieProducer : Entity
{
  private MovieProducer(
    Guid id,
    MovieId movieId,
    PersonId actorId)
    : base(id)
  {
    Id = id;
    MovieId = Guard.Against.Null(movieId);
    ProducerId = Guard.Against.Null(actorId);
  }

  private MovieProducer()
  {
  }

  public MovieId MovieId { get; private set; } = default!;

  public Movie Movie { get; private set; } = default!;

  public PersonId ProducerId { get; private set; } = default!;

  public Person Producer { get; private set; } = default!;

  public static MovieProducer Create(
    MovieId movieId,
    PersonId actorId,
    Guid? id = null)
  {
    return new MovieProducer(
      movieId: movieId,
      actorId: actorId,
      id: id ?? Guid.NewGuid());
  }
}
