namespace ScreenDrafts.Modules.Movies.Domain.Movies.Entities;

public sealed class Genre : Entity
{
  private readonly List<MovieGenre> _movieGenres = [];

  private Genre(
    string name,
    Guid? id = null)
    : base(id ?? Guid.NewGuid())
  {
    Name = Guard.Against.NullOrWhiteSpace(name);
  }

  private Genre()
  {
  }

  public IReadOnlyList<MovieGenre> MovieGenres => _movieGenres.AsReadOnly();

  public string Name { get; private set; } = default!;

  public static Genre Create(string name, Guid? id = null)
  {
    return new Genre(name, id);
  }
}
