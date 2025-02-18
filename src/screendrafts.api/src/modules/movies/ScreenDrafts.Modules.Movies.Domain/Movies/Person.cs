namespace ScreenDrafts.Modules.Movies.Domain.Movies;

public sealed class Person : AggrgateRoot<PersonId, Guid>
{
  private readonly List<MovieActor> _movieActors = [];
  private readonly List<MovieWriter> _movieWriters = [];
  private readonly List<MovieDirector> _movieDirectors = [];
  private readonly List<MovieProducer> _movieProducers = [];

  private Person(
    string imdbId,
    string name,
    PersonId? id = null)
  {
    Id = id ?? PersonId.CreateUnique();
    ImdbId = imdbId;
    Name = name;
  }

  private Person()
  {
  }

  public string ImdbId { get; private set; } = default!;
  public string Name { get; private set; } = default!;

  public IReadOnlyList<MovieActor> MovieActors => _movieActors.AsReadOnly();
  public IReadOnlyList<MovieWriter> MovieWriters => _movieWriters.AsReadOnly();
  public IReadOnlyList<MovieDirector> MovieDirectors => _movieDirectors.AsReadOnly();
  public IReadOnlyList<MovieProducer> MovieProducers => _movieProducers.AsReadOnly();

  public static Person Create(
    string imdbId,
    string name)
  {
    return new Person(imdbId, name);
  }

  public void AddMovieActor(MovieActor movieActor)
  {
    _movieActors.Add(movieActor);
  }

  public void AddMovieWriter(MovieWriter movieWriter)
  {
    _movieWriters.Add(movieWriter);
  }

  public void AddMovieDirector(MovieDirector movieDirector)
  {
    _movieDirectors.Add(movieDirector);
  }

  public void AddMovieProducer(MovieProducer movieProducer)
  {
    _movieProducers.Add(movieProducer);
  }
}
