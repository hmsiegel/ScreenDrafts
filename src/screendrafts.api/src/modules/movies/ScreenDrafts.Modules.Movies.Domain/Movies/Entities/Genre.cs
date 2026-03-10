namespace ScreenDrafts.Modules.Movies.Domain.Movies.Entities;

public sealed class Genre : Entity
{
  private readonly List<MovieGenre> _movieGenres = [];

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

  public IReadOnlyList<MovieGenre> MovieGenres => _movieGenres.AsReadOnly();

  public string Name { get; private set; } = default!;
  public int TmdbId { get; private set; }

  public static Genre Create(string name, int tmdbId, Guid? id = null)
  {
    return new Genre(name, tmdbId, id);
  }
}
