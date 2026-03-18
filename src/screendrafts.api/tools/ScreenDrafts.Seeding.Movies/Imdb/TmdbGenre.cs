namespace ScreenDrafts.Seeding.Movies.Imdb;

public sealed record TmdbGenre
{
  public int Id { get; init; }
  public string Name { get; init; } = default!;
}
