namespace ScreenDrafts.Seeding.Movies.Imdb.GetOnlineMovie;

public sealed class ActorModel(
  string name, string imdbId)
{
  public string ImdbId { get; init; } = imdbId;
  public string Name { get; init; } = name;
}
