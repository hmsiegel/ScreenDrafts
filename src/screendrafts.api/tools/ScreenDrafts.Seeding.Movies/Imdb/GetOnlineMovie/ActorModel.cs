namespace ScreenDrafts.Seeding.Movies.Imdb.GetOnlineMovie;

public sealed class ActorModel(
  string name, string imdbId, int tmdbId)
{
  public string ImdbId { get; init; } = imdbId;
  public int TmdbId { get; init; } = tmdbId;
  public string Name { get; init; } = name;
}
