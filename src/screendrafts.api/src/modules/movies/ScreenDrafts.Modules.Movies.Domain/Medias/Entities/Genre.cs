namespace ScreenDrafts.Modules.Movies.Domain.Medias.Entities;

public sealed class Genre : Entity
{
  private readonly List<MediaGenre> _mediaGenres = [];

  private Genre(
    string name,
    int tmdbId,
    Guid? id = null)
    : base(id ?? Guid.NewGuid())
  {
    Name = Guard.Against.NullOrWhiteSpace(name);
    TmdbId = tmdbId;
  }

  private Genre()
  {
  }

  public IReadOnlyList<MediaGenre> MediaGenres => _mediaGenres.AsReadOnly();

  public string Name { get; private set; } = default!;
  public int TmdbId { get; private set; }

  public static Genre Create(string name, int tmdbId, Guid? id = null)
  {
    return new Genre(name, tmdbId, id);
  }
}
