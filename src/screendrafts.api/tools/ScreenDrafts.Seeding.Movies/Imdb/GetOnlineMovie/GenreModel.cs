namespace ScreenDrafts.Seeding.Movies.Imdb.GetOnlineMovie;

public sealed class GenreModel(int tmdb, string name)
{
  public int Tmdb { get; init; } = tmdb;
  public string Name { get; init; } = name;
}
