namespace ScreenDrafts.Modules.Movies.Domain.Medias;

public sealed class Person : AggregateRoot<PersonId, Guid>
{
  private readonly List<MediaActor> _mediaActors = [];
  private readonly List<MediaWriter> _mediaWriters = [];
  private readonly List<MediaDirector> _mediaDirectors = [];
  private readonly List<MediaProducer> _mediaProducers = [];

  private Person(
    string imdbId,
    string name,
    int tmdbId,
    PersonId? id = null)
  {
    Id = id ?? PersonId.CreateUnique();
    ImdbId = Guard.Against.NullOrWhiteSpace(imdbId);
    Name = Guard.Against.NullOrWhiteSpace(name);
    TmdbId = tmdbId;
  }

  private Person()
  {
  }

  public string ImdbId { get; private set; } = default!;
  public int TmdbId { get; private set; }
  public string Name { get; private set; } = default!;

  public IReadOnlyList<MediaActor> MediaActors => _mediaActors.AsReadOnly();
  public IReadOnlyList<MediaWriter> MediaWriters => _mediaWriters.AsReadOnly();
  public IReadOnlyList<MediaDirector> MediaDirectors => _mediaDirectors.AsReadOnly();
  public IReadOnlyList<MediaProducer> MediaProducers => _mediaProducers.AsReadOnly();

  public static Person Create(
    string imdbId,
    string name,
    int tmdbId,
    PersonId? id = null)
  {
    return new Person(
      imdbId: imdbId,
      name: name,
      tmdbId: tmdbId,
      id: id);
  }

  public void AddMediaActor(MediaActor mediaActor)
  {
    _mediaActors.Add(mediaActor);
  }

  public void AddMediaWriter(MediaWriter mediaWriter)
  {
    _mediaWriters.Add(mediaWriter);
  }

  public void AddMediaDirector(MediaDirector mediaDirector)
  {
    _mediaDirectors.Add(mediaDirector);
  }

  public void AddMediaProducer(MediaProducer mediaProducer)
  {
    _mediaProducers.Add(mediaProducer);
  }
}
